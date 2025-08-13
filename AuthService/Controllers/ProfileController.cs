using AuthService.Context;
using AuthService.Entities;
using AuthService.Request;
using Common.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers
{
    public class ProfileController : BaseAuthController
    {
        private readonly AuthServiceDBContext dBContext;

        public ProfileController(AuthServiceDBContext ctx)
        {
            dBContext = ctx;
        }

        [HttpPost]
        [Route("/GetProfileNames")]
        public async Task<Dictionary<long, string?>> GetProfileNames([FromBody] GetProfileNamesRequest request)
        {
            var users = await dBContext.Users.Where(u => request.userIDs.Contains(u.Id)).ToListAsync();

            return users.ToDictionary(u => u.Id, u => u.UserName);
        }
    }
}
