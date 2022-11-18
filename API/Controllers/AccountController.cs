using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private TokenService _tokenService { get; }
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, TokenService tokenService)
        {
            this._tokenService = tokenService;
            this._signInManager = signInManager;
            this._userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> login(LoginDto loginDto){
            var user = await this._userManager.FindByEmailAsync(loginDto.Email);
            if(user == null) return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password,false);
            if(result.Succeeded){
                return createUserObject(user);
            }

            return Unauthorized();
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto){
            
            if(_userManager.Users.Any(x => x.Email == registerDto.Email)) {
                ModelState.AddModelError("email", "Email Taken");
                return ValidationProblem();
            }
            if(_userManager.Users.Any(x => x.UserName == registerDto.Username)) {
                ModelState.AddModelError("username", "Username Taken");
                return ValidationProblem();
            }

            var user = new AppUser {
                DisplayName     = registerDto.DisplayName,
                Email           = registerDto.Email,
                UserName        = registerDto.Username
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if(result.Succeeded){
                return createUserObject(user);
            }

            return BadRequest("Problem registering user");
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser(){
            var user = await _userManager.FindByEmailAsync(User.FindFirstValue(ClaimTypes.Email));
            return createUserObject(user);
        }

        private UserDto createUserObject(AppUser user){
             return new UserDto {
                    DisplayName     = user.DisplayName,
                    Image           = null,
                    Token           = _tokenService.CreateToken(user),
                    Username        = user.UserName

                };
        }
    }

    

}