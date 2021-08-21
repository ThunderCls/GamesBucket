using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GamesBucket.App.Models;
using GamesBucket.DataAccess.Models;
using GamesBucket.DataAccess.Models.Dtos;
using GamesBucket.DataAccess.Services.ApiService;
using GamesBucket.DataAccess.Services.GameService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GamesBucket.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGameService _gameService;
        private readonly IApiService _apiService;

        public HomeController(ILogger<HomeController> logger, IGameService gameService, IApiService apiService)
        {
            _logger = logger;
            _gameService = gameService;
            _apiService = apiService;
        }

        public IActionResult New()
        {
            return View();
        }
        
        public async Task<IActionResult> Index()
        {
            var games = await _gameService.GetGames();
            var summary = new MainSummary
            {
                TotalGames = games.Count,
                TotalHours = Math.Ceiling(games.Sum(g => g.GameplayMainExtra)),
                TotalHoursPlayed = Math.Ceiling(games
                    .Where(g => g.Played)
                    .Sum(g => g.GameplayMainExtra))
            };
            
            return View(summary);
        }

        public async Task<IActionResult> Search(string query)
        {
            var gameResults = new List<SearchResult>(); 
            if (!string.IsNullOrEmpty(query))
            {
                gameResults = await _apiService.SearchGameByTitle(query);
            }
            
            return View(gameResults);
        }

        public async Task<IActionResult> Details(uint appId)
        {
            var gameDetails = new Game();
            if (appId > 0)
            {
                if (await _gameService.GameExistsBySteamAppId(appId))
                {
                    gameDetails = await _gameService.GetGameBySteamAppId(appId);
                    gameDetails.InCatalog = true;
                    return View(gameDetails);
                }
                
                gameDetails = await _apiService.GetGameDetails(appId);
            }

            return View(gameDetails);
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

    }
}