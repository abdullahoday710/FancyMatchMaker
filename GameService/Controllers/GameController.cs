using Common.Controllers;
using GameService.Request;
using GameService.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameService.Controllers
{
    public class GameController : BaseAuthController
    {
        private readonly OngoingGameService _gameService;

        public GameController(OngoingGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost]
        [Route("SetStance")]
        public async Task<IActionResult> SetStance([FromBody] SetStanceRequest request)
        {
            if (UserId != null)
            {
                await _gameService.SetPlayerStance(request.Stance, UserId.Value);

                return Ok();
            }
            return Unauthorized();
        }
    }
}
