using System.Linq;
using AutoMapper;
using DatingApp.API.DTOs;
using DatingApp.API.Models;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>().ForMember(dest => dest.PhotoUrl, 
                    opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
                    .ForMember(dst => dst.Age, opt =>opt.MapFrom(src => src.DateOfBirth.CalculateAge()));

            CreateMap<User, UserForDetailsDto>().ForMember(dest => dest.PhotoUrl, 
                    opt => opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
                    .ForMember(dst => dst.Age, opt =>opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
                    
            CreateMap<Photo, PhotosForDetailedDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<PhotoForCreationDto, Photo>();
            CreateMap<UserForRegisterDtos, User>();
            CreateMap<MessageForCreationDto, Message>().ReverseMap();
            CreateMap<Message, MessageToReturnDto>()
                    .ForMember(dest =>dest.SenderPhotoUrl, opt => opt.MapFrom(u =>u.Sender.Photos.FirstOrDefault(p =>p.IsMain).Url))
                    .ForMember(dest =>dest.RecipientPhotoUrl, opt => opt.MapFrom(u =>u.Recipient.Photos.FirstOrDefault(p =>p.IsMain).Url));
        }
        
    }
}