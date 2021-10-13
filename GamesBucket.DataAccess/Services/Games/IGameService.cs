using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GamesBucket.DataAccess.Models;
using GamesBucket.Shared.Helpers;
using Microsoft.AspNetCore.Identity;

namespace GamesBucket.DataAccess.Services.Games
{
    public interface IGameService
    {
        Task<List<Game>> GetGames(AppUser user);
        Task<PagedResult<Game>> GetFilteredGamesPage(AppUser user, int pageSize, int page, string beatTimeInitial,  
            string beatTimeFinal, string genres, string sortBy, string sortType, string gameTitle);
        Task<Game> GetGameByName(string name);
        Task<Game> GetGameBySteamAppId(uint steamAppId, AppUser user);
        Task<Game> GetGameById(Guid gameId, AppUser user);
        Task Add(Game game);
        Task<bool> SetFavoriteStatusBySteamAppId(uint appId, AppUser user);
        Task<bool> SetFavoriteStatusByGameId(Guid gameId, AppUser user);
        Task AddCustomGame(Game newGame);
        Task RemoveBySteamAppId(long appId, AppUser user);
        Task<Game> Update(Game game);
        Task<bool> GameExistsBySteamAppId(long appId, IdentityUser user);
        Task<bool> GameExistsByGameId(Guid gameId);
        Task RemoveByGameId(Guid gameId, AppUser user);
        Task<List<Game>> SearchGamesByTitle(string gameTitle);
        Task<bool> SetCompleteStatusBySteamAppId(uint steamAppId, AppUser user);
        Task<bool> SetCompleteStatusByGameId(Guid gameId, AppUser user);
    }
}
