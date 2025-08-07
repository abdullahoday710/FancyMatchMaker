using Common.Controllers;
using MatchmakingService.Context;
using MatchmakingService.Entities;
using MatchmakingService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchmakingService.Controllers
{
    public class MatchMakingController : BaseAuthController
    {
        private readonly MatchMakingServiceDBContext _dbContext;
        private readonly MatchMakerService _matchmakingService;

        public MatchMakingController(MatchMakingServiceDBContext ctx, MatchMakerService matchmaker)
        {
            _dbContext = ctx;
            _matchmakingService = matchmaker;
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

        [HttpPost]
        [Route("JoinQueue")]
        public async Task<IActionResult> JoinQueue()
        {
            if (UserId != null)
            {
                MatchMakingProfileEntity profile;
                profile = await _dbContext.MatchMakingProfiles.Where(p => p.UserID == UserId).FirstOrDefaultAsync();
                await _matchmakingService.EnqueuePlayerAsync(profile);
                return Ok("Player added to queue.");
            }

            return Unauthorized();
        }

        [HttpPost]
        [Route("LeaveQueue")]
        public async Task<IActionResult> LeaveQueue()
        {
            if (UserId != null)
            {
                await _matchmakingService.RemovePlayerFromQueue(UserId.Value);
                return Ok("Player removed from queue.");
            }

            return Unauthorized();
        }

    }
}
