using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMapper _mapper;
        public AuthController(  IConfiguration config, 
                                IMapper mapper,
                                UserManager<User> userManager, 
                                SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _config = config;
        }


    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(UserForRegisterDtos userForRegister)
    {

        var userToCreate = _mapper.Map<User>(userForRegister);
        var result = await _userManager.CreateAsync(userToCreate, userForRegister.Password);
        var userToReturn = _mapper.Map<UserForDetailsDto>(userToCreate);

        if(result.Succeeded)
        {
            return CreatedAtRoute("GetUser", new { controller = "Users", id = userToCreate.Id }, userToReturn);
        }
        return BadRequest(result.Errors);


    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
    {

        var user = await _userManager.FindByNameAsync(userForLoginDto.Username);
        var result = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDto.Password, false);

        if(result.Succeeded)
        {
            if (user == null)
                return Unauthorized();

            var appUser = _mapper.Map<UserForListDto>(user);

            return Ok(new
            {
                token = GenerateJwtToken(user).Result,
                user = appUser
            });
        }
        return Unauthorized();
    }

    private async Task<string> GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
        };

        var roles = await _userManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDiscriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(1),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDiscriptor);
        return tokenHandler.WriteToken(token);
    }

}
}