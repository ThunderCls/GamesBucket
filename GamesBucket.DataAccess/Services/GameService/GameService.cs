using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GamesBucket.DataAccess.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace GamesBucket.DataAccess.Services.GameService
{
    public class GameService : IGameService
    {
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GameService(AppDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<Game>> GetGames()
        {
            return await _dbContext.Games
                .Include(g => g.Genres)
                .Include(g => g.Movies)
                .Include(g => g.Screenshots)
                .ToListAsync();
        }

        public async Task<Game> GetGameByName(string name)
        {
            return await _dbContext.Games
                .Include(g => g.Genres)
                .Include(g => g.Movies)
                .Include(g => g.Screenshots)
                .FirstOrDefaultAsync(g =>
                g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Game> GetGameBySteamAppId(uint steamAppId)
        {
            return await _dbContext.Games
                .Include(g => g.Genres)
                .Include(g => g.Movies)
                .Include(g => g.Screenshots)
                .FirstOrDefaultAsync(g =>
                g.SteamAppId == steamAppId);
        }
        
        public async Task<Game> GetGameById(Guid gameId)
        {
            return await _dbContext.Games
                .Include(g => g.Genres)
                .Include(g => g.Movies)
                .Include(g => g.Screenshots)
                .FirstOrDefaultAsync(g =>
                    g.GameId == gameId);
        }

        public async Task Add(Game game)
        {
            var result = await _dbContext.Games
                .FirstOrDefaultAsync(g => g.SteamAppId == game.SteamAppId);
            if (result == null)
            {
                game.GameId = Guid.NewGuid();
                await ValidateUniqueGenres(game);
                _dbContext.Games.Add(game);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task ValidateUniqueGenres(Game game)
        {
            // algorithm to avoid unique constraint violation
            var dbGenres = await _dbContext.Genres.AsNoTracking().ToListAsync();
            game.Genres.ToList().ForEach(g =>
            {
                g.GenreId = dbGenres.FirstOrDefault(dbG =>
                    dbG.Name.Equals(g.Name, StringComparison.OrdinalIgnoreCase))?.GenreId ?? 0;
                if (g.GenreId != 0)
                {
                    var genreEntry = _dbContext.Genres.Attach(g);
                    genreEntry.State = EntityState.Unchanged;
                }
            });
        }

        // https://blog.elmah.io/upload-and-resize-an-image-with-asp-net-core-and-imagesharp/
        public async Task AddCustomGame(Game newGame)
        {
            SetCoverImage(newGame);
            newGame.GameId = Guid.NewGuid();
            await ValidateUniqueGenres(newGame);
            _dbContext.Games.Add(newGame);
            await _dbContext.SaveChangesAsync();
        }

        private void SetCoverImage(Game newGame)
        {
            if (newGame.CoverPhoto == null) return;
            
            var uniqueFileName = Guid.NewGuid() + ".jpeg";//Path.GetExtension(userProfilePhoto.Photo.FileName);
            newGame.BoxImage = @$"\img\covers\{uniqueFileName}";
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img\\covers");
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
            using var image = Image.Load(newGame.CoverPhoto.OpenReadStream());
            // resize picture
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Compand = true,
                Sampler = KnownResamplers.Lanczos3,
                Mode = ResizeMode.Max, // keep aspect ratio
                Size = new Size { Height = 450 }
            }));
            // copy the file to wwwroot/images/profile folder
            image.Save(filePath, new JpegEncoder()
            {
                Quality = 50 
                // Quality index must be between 0 and 100 (compression from max to min). Defaults to 75.
            });
        }

        public async Task RemoveBySteamAppId(long appId)
        {
            var result = await _dbContext.Games.FirstOrDefaultAsync(g => g.SteamAppId == appId);
            if (result != null && result.SteamAppId != 0)
            {
                _dbContext.Games.Remove(result);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task RemoveByGameId(Guid gameId)
        {
            var result = await _dbContext.Games.FirstOrDefaultAsync(g => g.GameId == gameId);
            if (result?.GameId != null)
            {
                var coverImage = _webHostEnvironment.WebRootPath + result.BoxImage;
                File.Delete(coverImage);
                _dbContext.Games.Remove(result);
                await _dbContext.SaveChangesAsync();
            }
        }
        
        public async Task<Game> Update(Game game)
        {
            var gameResult = _dbContext.Games.Attach(game);
            gameResult.State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            
            return game;
        }
        
        public async Task<bool> GameExistsBySteamAppId(long appId)
        {
            var game = await _dbContext.Games.FirstOrDefaultAsync(g => g.SteamAppId == appId);
            return (game != null && game.SteamAppId != 0);
        }
        
        public async Task<bool> GameExistsByGameId(Guid gameId)
        {
            var game = await _dbContext.Games.FirstOrDefaultAsync(g => g.GameId == gameId);
            return (game?.GameId != null);
        }
        
        public async Task<List<Game>> SearchGamesByTitle(string gameTitle)
        {
            return await _dbContext.Games.Where(g => 
                    EF.Functions.Like(g.Name, $"%{gameTitle}%"))
                .ToListAsync();
        }
    }
}