using System.Threading.Tasks;
using GamesBucket.DataAccess.Models;

namespace GamesBucket.DataAccess.Services.Api.HLTB
{
    public interface IHltbService
    {
        Task<Game> GetHltbGameInfo(Game game);
    }
}