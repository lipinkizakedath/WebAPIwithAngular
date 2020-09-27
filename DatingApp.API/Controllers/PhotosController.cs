using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(
            IDatingRepository repo, 
            IMapper mapper,
            IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _repo = repo;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;


            Account acc = new Account
            (
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }


        [HttpGet("{id}", Name= "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);
            return Ok(photo);   
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(userId, true);

            var file = photoForCreationDto.File;

            var uploadResult = new ImageUploadResult();
            if(file.Length >0)
            {
                using(var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File =  new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.publicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationDto);

            // if(!userFromRepo.Photos.Any(u =>u.IsMain))
            // {
            //     if(userFromRepo.Photos.Any(p => p.IsApproved))
            //     photo.IsMain = true;
            // }

            userFromRepo.Photos.Add(photo);

            if(await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", 
                        new{ userId = userId, id = photo.Id}, photoToReturn);
            }
                
            return BadRequest("Photo not added, something went wrong!");

        }


        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var isCurrentUser = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) == userId;

            var user = await _repo.GetUser(userId, isCurrentUser);

            if(!user.Photos.Any(p => p.Id == id))
                return Unauthorized();
            
            var photoFromRepo =  await _repo.GetPhoto(id);

            if(photoFromRepo.IsMain)
                return BadRequest("This is already the main photo");

            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
            
            currentMainPhoto.IsMain = false;
            photoFromRepo.IsMain = true;

            if(await _repo.SaveAll())
                return NoContent();
            
            return BadRequest("Could not set this photo as main");
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId, true);

            if(!user.Photos.Any(p => p.Id == id))
                return Unauthorized();
            
            var photoFromRepo =  await _repo.GetPhoto(id);

            if(photoFromRepo.IsMain)
                return BadRequest("Cannot delete your main photo!");


            if(photoFromRepo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);
                var result = _cloudinary.Destroy(deleteParams);

                if(result.Result == "ok")
                {
                    _repo.Delete(photoFromRepo);
                }
            }

            if(photoFromRepo.PublicId == null)
            {
                _repo.Delete(photoFromRepo);
            }
  
            if(await _repo.SaveAll())
                return Ok();
            
            return BadRequest("Failed to delete the photo!");

        }


        [HttpGet]
        [Route("GetUnapprovedPhotos")]
        [Authorize(Policy = "ModeratePhotoRole")]
        public async Task<IActionResult> GetUnApprovedPhotos(int userId)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var notApprovedPhotos = await _repo.GetUnApprovedPhotos();

            if(notApprovedPhotos == null)
                return NoContent();
            
            return Ok(notApprovedPhotos);
        }


        [HttpPost]
        [Route("ApprovePendingPhotos/{photoId}")]
        [Authorize(Policy = "ModeratePhotoRole")]
        public async Task<IActionResult> ApprovePendingPhotos(int userId, [FromRoute]int photoId)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var pendingPhoto = await _repo.GetNotApprovedPhoto(photoId);

            if(pendingPhoto == null)
                return BadRequest("This is alreay an approved photo!");

            if(!pendingPhoto.IsApproved)
            {
                pendingPhoto.IsApproved = true;
                
                await _repo.SaveAll();
                return Ok();
            }
            return BadRequest();

        }


        [HttpPost("rejectPhoto/{photoId}")]
        [Authorize(Policy = "ModeratePhotoRole")]
        public async Task<IActionResult> RejectPendingPhotos(int photoId)
        {
            var photoToReject = await _repo.RejectPhotos(photoId);

            if(photoToReject == null)
                return BadRequest("No such photo exist!");

            if(photoToReject.IsMain || photoToReject.IsApproved)
                return BadRequest("Approved photos can be delete only the user");

            if(photoToReject.PublicId != null)
            {
                var deleteParams = new DeletionParams(photoToReject.PublicId);
                var result = _cloudinary.Destroy(deleteParams);

                if(result.Result == "ok")
                {
                    _repo.Delete(photoToReject);

                    if(await _repo.SaveAll())
                    {
                        return Ok();
                    }

                }
                return BadRequest("Not foud in cloud!");

            }

            if(photoToReject.PublicId == null)
            {
                _repo.Delete(photoToReject);

                if(await _repo.SaveAll())
                {
                    return Ok();
                }
            }

            return BadRequest("Could not delete photo!");
            
            
        }


    }
}