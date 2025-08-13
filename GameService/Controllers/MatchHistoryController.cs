using Common.Controllers;
using Common.Repo;
using GameService.Repo;
using GameService.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameService.Controllers
{
    public class MatchHistoryController : BaseAuthController
    {
        private readonly MatchHistoryService _matchHistoryService;

        public MatchHistoryController(MatchHistoryService matchHistoryService)
        {
            _matchHistoryService = matchHistoryService;
        }

        [HttpGet]
        [Route("GetMatchHistory")]
        public async Task<IActionResult> GetMatches()
        {
            if (UserId != null)
            {
                var matches = await _matchHistoryService.GetPlayerMatches(UserId.Value);
                return Ok(matches);
            }

            return Unauthorized();

        }
    }
}
