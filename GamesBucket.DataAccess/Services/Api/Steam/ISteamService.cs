using System.Collections.Generic;
using System.Threading.Tasks;
using GamesBucket.DataAccess.Models;
using GamesBucket.DataAccess.Models.Dtos;
using GamesBucket.DataAccess.Models.Steam;
using GamesBucket.Shared.Helpers;

namespace GamesBucket.DataAccess.Services.Api.Steam
{
    public interface ISteamService
    {
        Task<PagedResult<GameList.App>> SearchGameByTitle(string gameTitle, int page, int pageSize);
        Task<IList<SearchResult>> GetDetailedPagedResults(IList<GameList.App> baseResults);

        Task<Game> GetGameDetails(long appId);

        Task<SearchResult> GetGameInfo(GameList.App app);

        Task UpdateSteamLibrary();
    }
}