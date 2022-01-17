using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GamesBucket.DataAccess.Models;
using GamesBucket.DataAccess.Models.Dtos;
using GamesBucket.DataAccess.Models.Steam;
using GamesBucket.DataAccess.Services.Api.HLTB;
using GamesBucket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GamesBucket.DataAccess.Services.Api.Steam
{
    public class SteamService : ISteamService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<SteamService> _logger;
        private readonly IHltbService _hltbService;

        private readonly HttpClient _httpClient;
        private readonly string _steamKey;

        private const int MaxResults = 1000;
        private const string GameListEndpoint = "https://api.steampowered.com/IStoreService/GetAppList/v1/?";
        private const string GameDetailsEndpoint = "https://store.steampowered.com/api/appdetails/?";
        private const string GameReviewsEndpoint = "https://store.steampowered.com/appreviews/";
        
        public SteamService(IConfiguration configuration, 
            HttpClient httpClient,
            AppDbContext dbContext, 
            ILogger<SteamService> logger,
            IHltbService hltbService)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _logger = logger;
            _hltbService = hltbService;
            
            _steamKey = configuration.GetSection("SteamApi:Key").Value;
        }

        public async Task<PagedResult<GameList.App>> SearchGameByTitle(string gameTitle, int page, int pageSize)
        {
            var searchGamesList = new PagedResult<GameList.App>();
            var appsList = await GetGameIdsByTitleLocal(gameTitle);
            if (!appsList.Any()) return searchGamesList;

            searchGamesList = appsList.GetPaged(page, pageSize);
            return searchGamesList;
        }

        public async Task<IList<SearchResult>> GetDetailedPagedResults(IList<GameList.App> baseResults)
        {
            // run every task in "parallel"
            // var searchTasks = appsList.Select(GetGameInfo);
            // var results = await Task.WhenAll(searchTasks);
            return (await baseResults
                .ParallelForEachAsync(GetGameInfo, 10)).ToList();
        }
        
        public async Task<SearchResult> GetGameInfo(GameList.App app)
        {
            try
            {
                return new SearchResult
                {
                    SteamAppId = app.Appid,
                    Name = app.Name,
                    BoxImage = await GetBoxImageUrl(new GameDetails
                    {
                        GameData = new GameDetails.Data
                        {
                            Name = app.Name, SteamAppid = app.Appid
                        }
                    })
                };
                
                // var url = $"{GameDetailsEndpoint}appids={app.Appid}";
                // var details = await _httpClient.GetStringAsync(url);
                //
                // var gameDetails = DeserializeGameDetails(details);
                // if (gameDetails != null && gameDetails.Success)
                // {
                //     DateTime releaseDate = DateTime.MinValue;
                //     if (!gameDetails.GameData.ReleaseDate.ComingSoon)
                //     {
                //         DateTime.TryParse(gameDetails.GameData.ReleaseDate.Date, out releaseDate);
                //     }
                //
                //     return new SearchResult
                //     {
                //         SteamAppId = gameDetails.GameData.SteamAppid,
                //         Name = gameDetails.GameData.Name,
                //         Platforms = gameDetails.GameData.Platforms,
                //         BoxImage = await GetBoxImageUrl(gameDetails),
                //         HeaderImage = gameDetails.GameData.HeaderImage,
                //         ReleaseDate = releaseDate,
                //         Genres = gameDetails.GameData.Genres.Select(g => new Genres
                //         {
                //             Name = g.Description
                //         }).ToList()
                //     };
                // }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
            }
            
            // cool-down
            Thread.Sleep(500);
            
            return null;
        }

        private async Task<string> GetBoxImageUrl(GameDetails gameDetails)
        {
            var boxImageUrl =
                $"https://steamcdn-a.akamaihd.net/steam/apps/{gameDetails.GameData.SteamAppid}/library_600x900.jpg";
            var imageRequest = new HttpRequestMessage(HttpMethod.Head, boxImageUrl); // only receive headers
            var imageResult = await _httpClient.SendAsync(imageRequest);
            if (!imageResult.IsSuccessStatusCode)
            {
                var hltbData = await _hltbService.GetHltbGameInfo(new Game {Name = gameDetails.GameData.Name});
                boxImageUrl = !string.IsNullOrEmpty(hltbData.BoxImageHLTB)
                    ? hltbData.BoxImageHLTB
                    : String.Empty;
            }

            return boxImageUrl;
        }

        public async Task<Game> GetGameDetails(long appId)
        {
            try
            {
                // details
                var url = $"{GameDetailsEndpoint}appids={appId}";
                var details = await _httpClient.GetStringAsync(url);
                var gameDetails = DeserializeGameDetails(details);
                
                // reviews
                url = $"{GameReviewsEndpoint}{appId}?json=1&language=all";
                var reviews = await _httpClient.GetStringAsync(url);
                var gameReviews = DeserializeGameReviews(reviews);

                if (gameDetails != null && gameDetails.Success)
                {
                    DateTime releaseDate = DateTime.MinValue;
                    if (!gameDetails.GameData.ReleaseDate.ComingSoon)
                    {
                        DateTime.TryParse(gameDetails.GameData.ReleaseDate.Date, out releaseDate);
                    }
                    
                    var gameResult = new Game
                    {
                        SteamAppId = gameDetails.GameData.SteamAppid,
                        Name = gameDetails.GameData.Name,
                        Description = gameDetails.GameData.ShortDescription,
                        DetailedDescription = gameDetails.GameData.DetailedDescription,
                        AboutTheGame = gameDetails.GameData.AboutTheGame,
                        ShortDescription = gameDetails.GameData.ShortDescription,
                        SupportedLanguages = gameDetails.GameData.SupportedLanguages,
                        HeaderImage = gameDetails.GameData.HeaderImage,
                        Website = gameDetails.GameData.Website,
                        BackgroundImage = gameDetails.GameData.Background,
                        BoxImage = await GetBoxImageUrl(gameDetails),
                        MetaCriticScore = gameDetails.GameData.Metacritic.Score,
                        MetaCriticUrl = gameDetails.GameData.Metacritic.Url,
                        Linux = gameDetails.GameData.Platforms.Linux,
                        Mac = gameDetails.GameData.Platforms.Mac,
                        Windows = gameDetails.GameData.Platforms.Windows,
                        ReleaseDate = releaseDate,
                        Developers = gameDetails.GameData.Developers.Any() 
                            ? string.Join(", ", gameDetails.GameData.Developers)
                            : string.Empty,
                        SteamScore = gameReviews != null && gameReviews.Success == 1 
                                                         && gameReviews.Summary != null 
                            ? gameReviews.Summary.TotalPositive * 100 / gameReviews.Summary.TotalReviews
                            : -1,
                        Genres = gameDetails.GameData.Genres.Select(g => new Genres
                        {
                            Name = g.Description
                        }).ToList(),
                        Screenshots = gameDetails.GameData.Screenshots.Select(s => new Screenshots
                        {
                            PathFull = s.PathFull,
                            PathThumbnail = s.PathThumbnail
                        }).ToList(),
                        Movies = gameDetails.GameData.Movies.Select(m => new Movies
                        {
                            Name = m.Name,
                            Thumbnail = m.Thumbnail,
                            Webm = m.Webm.Max
                        }).ToList()
                    };

                    return await _hltbService.GetHltbGameInfo(gameResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
            }
            
            return null;
        }

        private GameReviews DeserializeGameReviews(string reviews)
        {
            // NOTE: extract only review score
            GameReviews gameReviews = null;
            if (!string.IsNullOrEmpty(reviews) && reviews.Contains(",\"reviews\":[{"))
            {
                reviews = reviews.Substring(0,
                    reviews.IndexOf(",\"reviews\":[{", StringComparison.Ordinal)) + "}";
                gameReviews = JsonSerializer.Deserialize<GameReviews>(reviews);
            }

            return gameReviews;
        }

        private GameDetails DeserializeGameDetails(string details)
        {
            // NOTE: Special steps when deserializing the AppDetails response since the response has a
            // dynamic property based on the app id which is passed as a parameter.
            // For example, passing app id "570" to the AppDetails endpoint will give a response like so:
            // "570": {
            //     "success": true
            //     "data": ...
            // }
            
            GameDetails gameDetails = null;
            if (!string.IsNullOrEmpty(details) && details.Contains(":"))
            {
                details = details.Substring(details.IndexOf(':') + 1,
                    details.Length - (details.IndexOf(':') + 2));
                // remove since some games treat these as arrays and throws an exception when serializing and empty
                details = details.Replace("\"linux_requirements\":[],", string.Empty);
                details = details.Replace("\"mac_requirements\":[],", string.Empty);

                gameDetails = JsonSerializer.Deserialize<GameDetails>(details);
            }

            return gameDetails;
        }

        private async Task<List<GameList.App>> GetGameIdsByTitleLocal(string gameTitle)
        {
            return await _dbContext.SteamLibraries.Where(g => 
                    EF.Functions.Like(g.Name, $"%{gameTitle}%"))
                .Select(g => new GameList.App()
                {
                    Appid = g.SteamAppId,
                    Name = g.Name
                }).ToListAsync();
        }
        
        private async Task<List<GameList.App>> GetGameIdsByTitleSteam(string gameTitle)
        {
            var appsList = new List<GameList.App>();
            var url = $"{GameListEndpoint}key={_steamKey}&max_results={MaxResults}";
            try
            {
                // search games by title
                GameList appList = null;
                do
                {
                    var games = await _httpClient.GetStringAsync(url);
                    appList = JsonSerializer.Deserialize<GameList>(games);

                    if (appList != null && appList.ApiResponse != null)
                    {
                        var searchResult = appList.ApiResponse.Apps.Where(a =>
                            a.Name.Contains(gameTitle, StringComparison.OrdinalIgnoreCase));
                        appsList.AddRange(searchResult); // save list of appids

                        if (appList.ApiResponse.HaveMoreResults)
                        {
                            url = $"{GameListEndpoint}key={_steamKey}" +
                                  $"&max_results={MaxResults}&last_appid={appList.ApiResponse.LastAppid}";
                        }
                    }

                    // cooldown
                    Thread.Sleep(50);
                } while (appList?.ApiResponse != null && appList.ApiResponse.HaveMoreResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
            }

            return appsList;
        }
        
        public async Task UpdateSteamLibrary()
        {
            var url = $"{GameListEndpoint}key={_steamKey}&max_results={MaxResults}";
            GameList appList = null;
            do
            {
                var games = await _httpClient.GetStringAsync(url);
                appList = JsonSerializer.Deserialize<GameList>(games);
                if (appList?.ApiResponse?.Apps != null)
                {
                    var gamesList = appList.ApiResponse.Apps
                        .Select(a => new SteamLibrary
                        {
                            SteamAppId = a.Appid,
                            Name = a.Name
                        });

                    await _dbContext.SteamLibraries.AddRangeAsync(gamesList);

                    if (appList.ApiResponse.HaveMoreResults)
                    {
                        url = $"{GameListEndpoint}key={_steamKey}" +
                              $"&max_results={MaxResults}&last_appid={appList.ApiResponse.LastAppid}";
                    }
                }
                
                // cooldown
                Thread.Sleep(100);
            } while (appList?.ApiResponse != null && appList.ApiResponse.HaveMoreResults);
            
            // clear the table before importing
            await _dbContext.Database.ExecuteSqlRawAsync("delete from SteamLibraries");
            
            await _dbContext.SaveChangesAsync();
        }
    }
}