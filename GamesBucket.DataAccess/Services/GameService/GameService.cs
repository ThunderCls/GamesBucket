using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GamesBucket.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace GamesBucket.DataAccess.Services.GameService
{
    public class GameService : IGameService
    {
        private readonly AppDbContext _dbContext;

        public GameService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Game>> GetGames()
        {
            return await _dbContext.Games.ToListAsync();
        }

        public async Task<Game> GetGameByName(string name)
        {
            return await _dbContext.Games.FirstOrDefaultAsync(g =>
                g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Game> GetGameBySteamAppId(string steamAppId)
        {
            if (!uint.TryParse(steamAppId, out var appId)) return null;
            
            return await _dbContext.Games.FirstOrDefaultAsync(g =>
                g.SteamAppId == appId);
        }

        public async Task AddGame(Game game)
        {
            var result = await _dbContext.Games.FirstOrDefaultAsync(g => g.SteamAppId == game.SteamAppId);
            if (result == null)
            {
                _dbContext.Games.Add(game);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}