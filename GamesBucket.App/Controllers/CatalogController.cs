using System;
using System.Threading.Tasks;
using GamesBucket.DataAccess.Services.ApiService;
using GamesBucket.DataAccess.Services.GameService;
using Microsoft.AspNetCore.Mvc;

namespace GamesBucket.App.Controllers
{
    public class Catalog : Controller
    {
        private readonly IGameService _gameService;
        private readonly IApiService _apiService;

        public Catalog(IGameService gameService, IApiService apiService)
        {
            _gameService = gameService;
            _apiService = apiService;
        }
        
        public async Task<IActionResult> Index()
        {
            var games = await _gameService.GetGames();
            return View(games);
        }

        [HttpPost]
        public async Task<IActionResult> AddGame(long appId)
        {
            if (appId == 0) return BadRequest("Missing appid");
            try
            {
                var game = await _apiService.GetGameDetails(appId);
                await _gameService.AddGame(game);
                return Created(String.Empty, null);
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }
        
        public async Task<IActionResult> UpdateLibrary()
        {
            try
            {
                await _apiService.UpdateSteamLibrary();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}