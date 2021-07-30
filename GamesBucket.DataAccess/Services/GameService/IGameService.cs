using System.Collections.Generic;
using System.Threading.Tasks;
using GamesBucket.DataAccess.Models;

namespace GamesBucket.DataAccess.Services.GameService
{
    public interface IGameService
    {
        Task<List<Game>> GetGames();
        Task<Game> GetGameByName(string name);
        Task<Game> GetGameBySteamAppId(string steamAppId);
        Task AddGame(Game game);
    }
}
