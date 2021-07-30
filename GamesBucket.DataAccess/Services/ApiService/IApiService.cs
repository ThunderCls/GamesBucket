using System.Collections.Generic;
using System.Threading.Tasks;
using GamesBucket.DataAccess.Models;
using GamesBucket.DataAccess.Models.Dtos;

namespace GamesBucket.DataAccess.Services.ApiService
{
    public interface IApiService
    {
        Task<List<SearchResult>> SearchGameByTitle(string gameTitle);

        Task<Game> GetGameDetails(long appId);

        Task<SearchResult> GetGameInfo(Steam.GameList.App app);

        Task UpdateSteamLibrary();
    }
}