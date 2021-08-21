using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GamesBucket.DataAccess.Models;
using GamesBucket.Shared.Models;

namespace GamesBucket.DataAccess.Services.GameService
{
    public interface IGameService
    {
        Task<List<Game>> GetGames();
        Task<Game> GetGameByName(string name);
        Task<Game> GetGameBySteamAppId(uint steamAppId);
        Task<Game> GetGameById(Guid gameId);
        Task Add(Game game);
        Task AddCustomGame(Game newGame);
        Task RemoveBySteamAppId(long appId);
        Task<Game> Update(Game game);
        Task<bool> GameExistsBySteamAppId(long appId);
        Task<bool> GameExistsByGameId(Guid gameId);
        Task RemoveByGameId(Guid gameId);
        Task<List<Game>> SearchGamesByTitle(string gameTitle);
    }
}
