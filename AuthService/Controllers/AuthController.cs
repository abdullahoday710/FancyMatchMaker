using AuthService.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AuthService.Entities;
using AuthService.JWT;

using Common.Response;
using Common.Messaging;
using AuthService.Request;
using DotNetCore.CAP;
using Common.Controllers;
using AuthService.Response;

namespace AuthService.Controllers
{
    public class AuthController : BaseAuthController
    {
        private readonly ICapPublisher _capPublisher;
        private readonly UserManager<UserEntity> _userManager;

        public AuthController(AuthServiceDBContext ctx, UserManager<UserEntity> userManager, ICapPublisher capPublisher)
        {
            _capPublisher = capPublisher;
            _userManager = userManager;
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            UserEntity user;
            if (request.UserEmail != "")
            {
                user = await _userManager.FindByEmailAsync(request.UserEmail);
                if (user != null && await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    var token = JWTUtils.GenerateJwtToken(user.Id.ToString(), user.Email, DateTime.UtcNow.AddHours(24));
                    LoginResponse response = new LoginResponse { AuthToken = token, Email = user.Email, UserName = user.UserName, UserId = user.Id };

                    return Ok(new BaseResponse(true, 200, "success", response));
                }
            }

            return Unauthorized();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.UserEmail);

            if (user != null)
            {
                return Ok(new BaseResponse(false, 400, "A user with that email already exists"));
            }

            user = new UserEntity {Email = request.UserEmail, UserName = request.UserName };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                var message = new UserRegisteredMessage { UserID = user.Id, UserEmail = user.Email, UserName = user.UserName };

                _capPublisher.Publish(TopicNames.NewUserRegistered, message);

                return Ok(new BaseResponse(true, 200, "User created successfully", user));
            }
 

            return BadRequest(new BaseResponse(false, 400, "An error has occured when creating the user", result.Errors));
        }

    }
}
