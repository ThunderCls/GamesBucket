using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GamesBucket.App.Models;
using GamesBucket.DataAccess.Models;
using GamesBucket.DataAccess.Models.Dtos;
using GamesBucket.DataAccess.Seed;
using GamesBucket.DataAccess.Services.Api;
using GamesBucket.DataAccess.Services.Api.Steam;
using GamesBucket.DataAccess.Services.Games;
using GamesBucket.DataAccess.Services.Users;
using GamesBucket.Shared.Helpers;
using GamesBucket.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GamesBucket.App.Controllers
{
    [Route("catalog")]
    public class Catalog : Controller
    {
        private readonly IGameService _gameService;
        private readonly ISteamService _apiService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        private const int MaxPageSize = 64;
        private const int MinPageSize = 12;

        public Catalog(IGameService gameService, 
            ISteamService apiService, IMapper mapper, IUserService userService)
        {
            _gameService = gameService;
            _apiService = apiService;
            _mapper = mapper;
            _userService = userService;
        }
        
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return RedirectToAction("Error", "Home");

            var games = await _gameService
                .GetFilteredGamesPage(loggedUser, MinPageSize, 1, null, 
                    null, null, null, null, null, null, null);
            
            return View(games);
        }

        [HttpGet("genres")]
        public async Task<IActionResult> GetAllGenres([FromQuery] string term)
        {
            var genres = !string.IsNullOrEmpty(term)
                ? await _gameService.GetGenres(term)
                : await _gameService.GetGenres(string.Empty, int.MaxValue);
            
            return Json(new { suggestions = genres.OrderBy(g => g.Name)
                .Select(g => g.Name) });
        }
        
        [HttpGet("activegenres")]
        public async Task<IActionResult> GetAllActiveGenres([FromQuery] string term)
        {
            var genres = !string.IsNullOrEmpty(term)
                ? await _gameService.GetActiveGenres(term)
                : await _gameService.GetActiveGenres(string.Empty, int.MaxValue);
            
            return Json(new { suggestions = genres.OrderBy(g => g.Name)
                .Select(g => g.Name) });
        }
        
        [HttpPost("filter")]
        public async Task<IActionResult> Filter(string beatTimeInitial, string beatTimeFinal, string genres, 
            string sortBy, string sortType, int page, int pageSize, string gameTitle, string completionStatus,
            string favoriteStatus)
        {
            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return NotFound("User not found");

            pageSize = pageSize == 0 ? MinPageSize : pageSize;
            pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;
            var games = await _gameService
                .GetFilteredGamesPage(loggedUser, pageSize, page, beatTimeInitial, beatTimeFinal, 
                    genres, sortBy, sortType, gameTitle, completionStatus, favoriteStatus);

            return PartialView("_CardsContainerPartial", games);
        }

        [HttpGet("details")]
        public async Task<IActionResult> Details(string appId)
        {
            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return RedirectToAction("Error", "Home");
            Game gameDetails = new Game();
            
            if (uint.TryParse(appId, out var steamAppId) && steamAppId != 0)
            {
                gameDetails = await _gameService.GetGameBySteamAppId(steamAppId, loggedUser);
            }
            else if (Guid.TryParse(appId, out var gameId))
            {
                gameDetails = await _gameService.GetGameById(gameId, loggedUser);
            }

            return View(gameDetails);
        }

        [HttpGet("custom")]
        public IActionResult Custom()
        {
            return View();
        }

        [HttpPost("custom")]
        public async Task<IActionResult> Custom(NewGameView newGame)
        {
            StringBuilder message = new StringBuilder();
            
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(modelState => 
                    modelState.Errors.Select(x => x)))
                {
                    message.AppendLine(error.ErrorMessage);
                }
                
                return BadRequest(message.ToString());
            }

            if (newGame.CoverPhoto != null &&
                (newGame.CoverPhoto.ContentType != "image/jpeg" &&
                 newGame.CoverPhoto.ContentType != "image/png"))
            {
                return BadRequest("Wrong file format!");
            }
            
            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return RedirectToAction("Error", "Home");
            
            var game = _mapper.Map<Game>(newGame);
            game = await _gameService.LoadGameGenres(game);
            game.AppUserId = loggedUser.Id;
            game.InCatalog = true;
            // todo: add try catch for possible errors when converting image and send an error description to the frontend
            await _gameService.AddCustomGame(game);
            
            return Created(String.Empty, null);
        }
        
        [HttpPost("save")]
        public async Task<IActionResult> Save(string appId)
        {
            if (!uint.TryParse(appId, out var steamAppId) || steamAppId == 0) 
                return BadRequest("Missing appId");
            
            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return RedirectToAction("Error", "Home");
            
            var game = await _apiService.GetGameDetails(steamAppId);
            game.AppUserId = loggedUser.Id;
            game.InCatalog = true;
            await _gameService.Add(game);
            
            return Created(String.Empty, null);
        }
        
        [HttpDelete("remove")]
        public async Task<IActionResult> Remove(string appId)
        {
            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return RedirectToAction("Error", "Home");
            
            if (uint.TryParse(appId, out var steamAppId) && steamAppId != 0)
            {
                await _gameService.RemoveBySteamAppId(steamAppId, loggedUser);
                return Ok();
            }
            
            if (!Guid.TryParse(appId, out var gameId)) 
                return BadRequest("Missing appId");
            
            await _gameService.RemoveByGameId(gameId, loggedUser);
            return Ok();
        }

        [HttpGet("edit")]
        public async Task<IActionResult> Edit(string appId)
        {
            Game gameDetails;
            EditGameViewModel editGameView;
            
            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return RedirectToAction("Error", "Home");
            
            if (uint.TryParse(appId, out var steamAppId) && steamAppId != 0)
            {
                gameDetails = await _gameService.GetGameBySteamAppId(steamAppId, loggedUser);
                if (gameDetails?.SteamAppId != 0)
                {
                    editGameView = _mapper.Map<EditGameViewModel>(gameDetails);
                    return View(editGameView);
                }
            }

            if (Guid.TryParse(appId, out var gameId))
            {
                gameDetails = await _gameService.GetGameById(gameId, loggedUser);
                if (gameDetails?.GameId != null)
                {
                    editGameView = _mapper.Map<EditGameViewModel>(gameDetails);
                    return View(editGameView);
                }
            }

            return View(new EditGameViewModel());
        }
        
        [HttpPost("edit")]
        public async Task<IActionResult> Edit(EditGameViewModel editGameView)
        {
            StringBuilder message = new StringBuilder();
            
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(modelState => 
                    modelState.Errors.Select(x => x)))
                {
                    message.AppendLine(error.ErrorMessage);
                }
                
                return BadRequest(message.ToString());
            }

            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return RedirectToAction("Error", "Home");
            
            var gameDetails = editGameView.SteamAppId > 0
                ? await _gameService.GetGameBySteamAppId(editGameView.SteamAppId, loggedUser)
                : await _gameService.GetGameById(editGameView.GameId, loggedUser);

            if(gameDetails?.GameId == null) return NotFound("Game not found");

            _mapper.Map(editGameView, gameDetails);
            gameDetails = await _gameService.LoadGameGenres(gameDetails);
            await _gameService.Update(gameDetails);

            var appId = gameDetails.SteamAppId == 0
                ? gameDetails.GameId.ToString()
                : gameDetails.SteamAppId.ToString();
            
            return RedirectToAction("Details", "Catalog", new {appId});
        }

        [HttpDelete("favorite")]
        public async Task<IActionResult> RemoveFavorite(string appId)
        {
            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return RedirectToAction("Error", "Home");
            
            if (uint.TryParse(appId, out var steamAppId) && steamAppId != 0)
            {
                var success = await _gameService.SetFavoriteStatusBySteamAppId(steamAppId, loggedUser);
                var favGames = (await _gameService.GetGames(loggedUser))
                    .Where(g => g.Favorite).ToList();
                if (success) return PartialView("_FavsListPartial", favGames);
            }

            if (Guid.TryParse(appId, out var gameId))
            {
                var success = await _gameService.SetFavoriteStatusByGameId(gameId, loggedUser);
                var favGames = (await _gameService.GetGames(loggedUser))
                    .Where(g => g.Favorite).ToList();
                if (success) return PartialView("_FavsListPartial", favGames);
            }
            
            return BadRequest();
        }
        
        [HttpPost("favorite")]
        public async Task<IActionResult> AddFavorite(string appId)
        {
            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return RedirectToAction("Error", "Home");
            
            if (uint.TryParse(appId, out var steamAppId) && steamAppId != 0)
            {
                var success = await _gameService.SetFavoriteStatusBySteamAppId(steamAppId, loggedUser);
                if(success) return Ok();
            }

            if (Guid.TryParse(appId, out var gameId))
            {
                var success = await _gameService.SetFavoriteStatusByGameId(gameId, loggedUser);
                if(success) return Ok();
            }
            
            return BadRequest();
        }
        
        [HttpDelete("played")]
        public async Task<IActionResult> RemovePlayed(string appId)
        {
            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return RedirectToAction("Error", "Home");
            
            if (uint.TryParse(appId, out var steamAppId) && steamAppId != 0)
            {
                var success = await _gameService.SetCompleteStatusBySteamAppId(steamAppId, loggedUser);
                var playedGames = (await _gameService.GetGames(loggedUser)).Where(g => g.Played).ToList();
                if (success) return PartialView("_CompletedListPartial", playedGames);
            }

            if (Guid.TryParse(appId, out var gameId))
            {
                var success = await _gameService.SetCompleteStatusByGameId(gameId, loggedUser);
                var playedGames = (await _gameService.GetGames(loggedUser)).Where(g => g.Played).ToList();
                if (success) return PartialView("_CompletedListPartial", playedGames);
            }
            
            return BadRequest();
        }
        
        [HttpPost("played")]
        public async Task<IActionResult> AddPlayed(string appId)
        {
            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return RedirectToAction("Error", "Home");
            
            if (uint.TryParse(appId, out var steamAppId) && steamAppId != 0)
            {
                var success = await _gameService.SetCompleteStatusBySteamAppId(steamAppId, loggedUser);
                if(success) return Ok();
            }

            if (Guid.TryParse(appId, out var gameId))
            {
                var success = await _gameService.SetCompleteStatusByGameId(gameId, loggedUser);
                if(success) return Ok();
            }
            
            return BadRequest();
        }
        
        [HttpGet("updatedb")]
        [Authorize(Roles = "Admin")]
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

        [HttpGet("search")]
        public async Task<IActionResult> Search(string query)
        {
            var gameResults = new PagedResult<SearchResult>(); 
            if (!string.IsNullOrEmpty(query))
            {
                var loggedUser = await _userService.GetLoggedUser(User);
                if (loggedUser == null) return NotFound("User not found");

                var games = await _gameService
                    .GetFilteredGamesPage(loggedUser, MinPageSize, 1, null, 
                        null, null, null, null, query, null, null);

                if (games.Results.Any())
                {
                    gameResults.Results = games.Results.Select(g => _mapper.Map<SearchResult>(g)).ToList();
                    gameResults.CurrentPage = games.CurrentPage;
                    gameResults.PageCount = games.PageCount;
                    gameResults.PageSize = games.PageSize;
                    gameResults.RowCount = games.RowCount;
                }
            }

            return View(gameResults);
        }

        [HttpPost("search/filter")]
        public async Task<IActionResult> SearchFilter(string sortBy, string sortType,
            int page, int pageSize, string gameTitle)
        {
            var gameResults = new PagedResult<SearchResult>(); 
            var loggedUser = await _userService.GetLoggedUser(User);
            if (loggedUser == null) return NotFound("User not found");

            pageSize = pageSize == 0 ? MinPageSize : pageSize;
            pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

            var games = await _gameService
                .GetFilteredGamesPage(loggedUser, pageSize, page, null, 
                    null, null, sortBy, sortType, gameTitle, null, null);

            if (games.Results.Any())
            {
                gameResults.Results = games.Results.Select(g => _mapper.Map<SearchResult>(g)).ToList();
                gameResults.CurrentPage = games.CurrentPage;
                gameResults.PageCount = games.PageCount;
                gameResults.PageSize = games.PageSize;
                gameResults.RowCount = games.RowCount;
            }
            
            return PartialView("_CardsSearchContainerPartial", gameResults);
        }
    }
}