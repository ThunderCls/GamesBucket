using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GamesBucket.App.Models;
using GamesBucket.DataAccess.Models;
using GamesBucket.DataAccess.Models.Dtos;
using GamesBucket.DataAccess.Services.Api;
using GamesBucket.DataAccess.Services.Api.Steam;
using GamesBucket.DataAccess.Services.Games;
using GamesBucket.DataAccess.Services.Users;
using GamesBucket.Shared.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GamesBucket.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGameService _gameService;
        private readonly ISteamService _apiService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserService _userService;

        private const int MaxPageSize = 60;
        private const int MinPageSize = 15;
        
        public HomeController(ILogger<HomeController> logger, 
            IGameService gameService, ISteamService apiService,
            UserManager<AppUser> userManager, IUserService userService)
        {
            _logger = logger;
            _gameService = gameService;
            _apiService = apiService;
            _userManager = userManager;
            _userService = userService;
        }

        public IActionResult New()
        {
            return View();
        }
        
        public async Task<IActionResult> Index()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return RedirectToAction("Error", "Home");
            
            var games = await _gameService.GetGames(loggedUser);
            var summary = new SummaryViewModel
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
            var gameResults = new PagedResult<SearchResult>(); 
            if (!string.IsNullOrEmpty(query))
            {
                var baseResults = await _apiService.SearchGameByTitle(query, 1, MinPageSize);
                if (baseResults.Results.Any())
                {
                    var searchResults = await _apiService
                        .GetDetailedPagedResults(baseResults.Results);
                    gameResults.Results = searchResults.Where(gameResult => gameResult != null).ToList();
                    gameResults.CurrentPage = baseResults.CurrentPage;
                    gameResults.PageCount = baseResults.PageCount;
                    gameResults.PageSize = baseResults.PageSize;
                    gameResults.RowCount = baseResults.RowCount;
                }
            }
            
            return View(gameResults);
        }
        
        [HttpPost]
        public async Task<IActionResult> SearchFilter(int page, int pageSize, string gameTitle)
        {
            var gameResults = new PagedResult<SearchResult>(); 
            if (!string.IsNullOrEmpty(gameTitle))
            {
                var baseResults = await _apiService.SearchGameByTitle(gameTitle, page, pageSize);
                if (baseResults.Results.Any())
                {
                    var searchResults = await _apiService
                        .GetDetailedPagedResults(baseResults.Results);
                    gameResults.Results = searchResults.Where(gameResult => gameResult != null).ToList();
                    gameResults.CurrentPage = baseResults.CurrentPage;
                    gameResults.PageCount = baseResults.PageCount;
                    gameResults.PageSize = baseResults.PageSize;
                    gameResults.RowCount = baseResults.RowCount;
                }
            }

            return PartialView("_CardsHomeSearchContainerPartial", gameResults);
        }

        public async Task<IActionResult> Details(uint appId)
        {
            var gameDetails = new Game();
            if (appId <= 0) return View(gameDetails);

            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return RedirectToAction("Error", "Home");
            
            if (await _gameService.GameExistsBySteamAppId(appId, loggedUser))
            {
                gameDetails = await _gameService.GetGameBySteamAppId(appId, loggedUser);
                gameDetails.InCatalog = true;
                return View(gameDetails);
            }
                
            gameDetails = await _apiService.GetGameDetails(appId);
            return View(gameDetails);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}