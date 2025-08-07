using Common.Controllers;
using MatchmakingService.Context;
using MatchmakingService.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MatchmakingService.Controllers
{
    public class MatchMakingController : BaseAuthController
    {
        private readonly MatchMakingServiceDBContext _dbContext;

        public MatchMakingController(MatchMakingServiceDBContext ctx)
        {
            _dbContext = ctx;
        }


        [HttpGet]
        [Route("MyProfile")]
        public async Task<IActionResult> MyProfile()
        {

            if (UserId != null)
            {
                MatchMakingProfileEntity profile;
                profile = await _dbContext.MatchMakingProfiles.Where(p => p.UserID == UserId).FirstOrDefaultAsync();
                return Ok(profile);
            }

            return Unauthorized();
            
        }

    }
}
