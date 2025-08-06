using AuthService.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AuthService.Entities;
using AuthService.JWT;

using Common.Response;
using AuthService.Request;

namespace AuthService.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class AuthController : ControllerBase
    {
        private readonly AuthServiceDBContext _context;
        private readonly UserManager<UserEntity> _userManager;

        public AuthController(AuthServiceDBContext ctx, UserManager<UserEntity> userManager)
        {
            _context = ctx;
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
                    return Ok(new BaseResponse(true, 200, "success", token));
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
                return Ok(new BaseResponse(true, 200, "User created successfully", user));

            return BadRequest(new BaseResponse(false, 400, "An error has occured when creating the user", result.Errors));
        }

    }
}
