using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GamesBucket.App.Models;
using GamesBucket.DataAccess.Models;
using GamesBucket.DataAccess.Models.Dtos;
using GamesBucket.DataAccess.Services.ApiService;
using GamesBucket.DataAccess.Services.GameService;
using GamesBucket.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GamesBucket.App.Controllers
{
    public class Catalog : Controller
    {
        private readonly IGameService _gameService;
        private readonly IApiService _apiService;
        private readonly IMapper _mapper;

        public Catalog(IGameService gameService, 
            IApiService apiService, IMapper mapper)
        {
            _gameService = gameService;
            _apiService = apiService;
            _mapper = mapper;
        }
        
        public async Task<IActionResult> Index()
        {
            var games = await _gameService.GetGames();
            return View(games);
        }
        
        public async Task<IActionResult> Details(string appId)
        {
            Game gameDetails = new Game();
            
            if (uint.TryParse(appId, out var steamAppId) && steamAppId != 0)
            {
                gameDetails = await _gameService.GetGameBySteamAppId(steamAppId);
            }
            else if (Guid.TryParse(appId, out var gameId))
            {
                gameDetails = await _gameService.GetGameById(gameId);
            }

            return View(gameDetails);
        }

        public IActionResult Custom()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Custom(NewGameView newGame)
        {
            if(!ModelState.IsValid) return BadRequest("Wrong data");

            if (newGame.CoverPhoto == null ||
                (newGame.CoverPhoto.ContentType != "image/jpeg" &&
                 newGame.CoverPhoto.ContentType != "image/png"))
            {
                return BadRequest("Wrong file format!");
            }
            
            var game = _mapper.Map<Game>(newGame);
            game.InCatalog = true;
            await _gameService.AddCustomGame(game);
            
            return Created(String.Empty, null);
        }
        
        [HttpPost]
        public async Task<IActionResult> Save(string appId)
        {
            if (!uint.TryParse(appId, out var steamAppId) || steamAppId == 0) 
                return BadRequest("Missing appId");
            
            var game = await _apiService.GetGameDetails(steamAppId);
            game.InCatalog = true;
            await _gameService.Add(game);
            
            return Created(String.Empty, null);
        }
        
        [HttpPost]
        public async Task<IActionResult> Remove(string appId)
        {
            if (uint.TryParse(appId, out var steamAppId) && steamAppId != 0)
            {
                await _gameService.RemoveBySteamAppId(steamAppId);
                return Ok();
            }
            
            if (!Guid.TryParse(appId, out var gameId)) 
                return BadRequest("Missing appId");
            
            await _gameService.RemoveByGameId(gameId);
            return Ok();
        }

        public async Task<IActionResult> EditGame(string appId)
        {
            Game gameDetails;
            EditGameView editGameView;
            
            if (uint.TryParse(appId, out var steamAppId) && steamAppId != 0)
            {
                gameDetails = await _gameService.GetGameBySteamAppId(steamAppId);
                if (gameDetails.SteamAppId != 0)
                {
                    editGameView = _mapper.Map<EditGameView>(gameDetails);
                    return View(editGameView);
                }
            }

            if (Guid.TryParse(appId, out var gameId))
            {
                gameDetails = await _gameService.GetGameById(gameId);
                if (gameDetails?.GameId != null)
                {
                    editGameView = _mapper.Map<EditGameView>(gameDetails);
                    return View(editGameView);
                }
            }

            return View(new EditGameView());
        }
        
        [HttpPost]
        public async Task<IActionResult> EditGame(EditGameView editGameView)
        {
            if(!ModelState.IsValid) return BadRequest("Wrong data");

            var gameDetails = editGameView.SteamAppId > 0
                ? await _gameService.GetGameBySteamAppId(editGameView.SteamAppId)
                : await _gameService.GetGameById(editGameView.GameId);

            if(gameDetails?.GameId == null) return NotFound("Game not found");

            _mapper.Map(editGameView, gameDetails);
            await _gameService.Update(gameDetails);

            var appId = gameDetails.SteamAppId == 0
                ? gameDetails.GameId.ToString()
                : gameDetails.SteamAppId.ToString();
            
            return RedirectToAction("Details", "Catalog", new {appId});
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

        public async Task<IActionResult> Search(string query)
        {
            var gameResults = new List<SearchResult>(); 
            if (!string.IsNullOrEmpty(query))
            {
                var games = await _gameService.SearchGamesByTitle(query);
                if(games.Any())
                    gameResults = games.Select(g => _mapper.Map<SearchResult>(g)).ToList();
            }
            
            return View(gameResults);
        }
    }
}