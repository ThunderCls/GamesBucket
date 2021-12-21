using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GamesBucket.DataAccess.Models;
using GamesBucket.Shared.Helpers;
using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace GamesBucket.DataAccess.Services.Games
{
    public class GameService : IGameService
    {
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private string CompleteStatus => "Completed";
        private string FavoriteStatus => "Favorite";

        public GameService(AppDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<Genres>> GetGenres(string term, int limit = 5)
        {
            var genres = await _dbContext.Genres.ToListAsync();
            
            return !string.IsNullOrEmpty(term)
                ? genres.Where(g => 
                    g.Name.Contains(term, StringComparison.InvariantCultureIgnoreCase)).Take(limit).ToList()
                : genres.Take(limit).ToList();
        }

        public async Task<List<Genres>> GetActiveGenres(string term, int limit = 5)
        {
            var activeGenres = await _dbContext.Games.SelectMany(a => a.Genres)
                .Distinct().ToListAsync();
            
            return !string.IsNullOrEmpty(term)
                ? activeGenres.Where(g => 
                    g.Name.Contains(term, StringComparison.InvariantCultureIgnoreCase)).Take(limit).ToList()
                : activeGenres.Take(limit).ToList();
        }

        public async Task<List<Models.Game>> GetGames()
        {
            return await _dbContext.Games
                .Include(g => g.Genres)
                .Include(g => g.Movies)
                .Include(g => g.Screenshots)
                .ToListAsync();
        }
        
        public async Task<List<Models.Game>> GetGames(AppUser user)
        {
            return await _dbContext.Games
                .Include(g => g.Genres)
                .Include(g => g.Movies)
                .Include(g => g.Screenshots)
                .Where(g => g.AppUserId == user.Id)
                .ToListAsync();
        }
        
        public async Task<PagedResult<Game>> GetFilteredGamesPage(AppUser user, int pageSize, int page,
            string beatTimeInitial, string beatTimeFinal, string genres, string sortBy, string sortType, 
            string gameTitle, string completionStatus, string favStatus)
        {
            var games = _dbContext.Games
                .Include(g => g.Genres)
                .Include(g => g.Movies)
                .Include(g => g.Screenshots)
                .Where(g => g.AppUserId == user.Id);

            // game title
            if (!string.IsNullOrEmpty(gameTitle))
            {
                games = games.Where(g =>
                    EF.Functions.Like(g.Name, $"%{gameTitle}%"));
            }
            
            // sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "release_date":
                        games = sortType.Equals("asc", StringComparison.OrdinalIgnoreCase) 
                            ? games.OrderBy(g => g.ReleaseDate)
                            : games.OrderByDescending(g => g.ReleaseDate);
                        break;
                    
                    case "beat_time":
                        games = sortType.Equals("asc", StringComparison.OrdinalIgnoreCase)
                            ? games.OrderBy(g => g.GameplayMainExtra)
                            : games.OrderByDescending(g => g.GameplayMainExtra);
                        break;
                    
                    case "game_score":
                        games = sortType.Equals("asc", StringComparison.OrdinalIgnoreCase)
                            ? games.OrderBy(g => g.SteamScore)
                            : games.OrderByDescending(g => g.SteamScore);
                        break;
                }
            }
            
            // beat time filter
            if (!string.IsNullOrEmpty(beatTimeInitial) && !string.IsNullOrEmpty(beatTimeFinal) &&
                double.TryParse(beatTimeInitial, out var beatTimeInitialFilter) &&
                double.TryParse(beatTimeFinal, out var beatTimeFinalFilter))
            {
                games = games.Where(g => 
                    g.GameplayMainExtra >= beatTimeInitialFilter && g.GameplayMainExtra <= beatTimeFinalFilter);
            }

            // completion status
            if (!string.IsNullOrEmpty(completionStatus))
            {
                games = games.Where(g =>
                    g.Played == completionStatus.Equals(CompleteStatus, StringComparison.OrdinalIgnoreCase));
            }

            // fav status
            if (!string.IsNullOrEmpty(favStatus))
            {
                games = games.Where(g =>
                    g.Favorite == favStatus.Equals(FavoriteStatus, StringComparison.OrdinalIgnoreCase));
            }

            // perform query on db
            var filteredGames = await games.ToListAsync();
            
            // genres filter
            if (!string.IsNullOrEmpty(genres))
            {
                var generesList = genres.Split(",").ToList();
                filteredGames = filteredGames.Where(g => 
                        generesList.Intersect(g.Genres.Select(x => x.Name.ToLower())).Any()).ToList();
            }

            return filteredGames.GetPaged(page, pageSize);
        }

        public async Task<Models.Game> GetGameByName(string name)
        {
            return await _dbContext.Games
                .Include(g => g.Genres)
                .Include(g => g.Movies)
                .Include(g => g.Screenshots)
                .FirstOrDefaultAsync(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Game> GetGameBySteamAppId(uint steamAppId, AppUser user)
        {
            return await _dbContext.Games
                .Include(g => g.Genres)
                .Include(g => g.Movies)
                .Include(g => g.Screenshots)
                .FirstOrDefaultAsync(g => g.SteamAppId == steamAppId && g.AppUserId == user.Id);
        }
        
        public async Task<Game> GetGameById(Guid gameId, AppUser user)
        {
            return await _dbContext.Games
                .Include(g => g.Genres)
                .Include(g => g.Movies)
                .Include(g => g.Screenshots)
                .FirstOrDefaultAsync(g => g.GameId == gameId && g.AppUserId == user.Id);
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

        public async Task<bool> SetFavoriteStatusBySteamAppId(uint appId, AppUser user)
        {
            var result = await _dbContext.Games
                .FirstOrDefaultAsync(g => g.SteamAppId == appId && g.AppUserId == user.Id);
            if (result == null) return false;

            return await SetFavoriteStatus(result);
        }
        
        public async Task<bool> SetFavoriteStatusByGameId(Guid gameId, AppUser user)
        {
            var result = await _dbContext.Games
                .FirstOrDefaultAsync(g => g.GameId == gameId && g.AppUserId == user.Id);
            if (result == null) return false;

            return await SetFavoriteStatus(result);
        }

        private async Task<bool> SetFavoriteStatus(Game result)
        {
            result.Favorite = !result.Favorite;
            _dbContext.Games.Update(result);
            await _dbContext.SaveChangesAsync();
            return true;
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

        public async Task RemoveBySteamAppId(long appId, AppUser user)
        {
            var result = await _dbContext.Games.FirstOrDefaultAsync(g => 
                g.SteamAppId == appId && g.AppUserId == user.Id);
            if (result != null && result.SteamAppId != 0)
            {
                _dbContext.Games.Remove(result);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task RemoveByGameId(Guid gameId, AppUser user)
        {
            var result = await _dbContext.Games.FirstOrDefaultAsync(g => 
                g.GameId == gameId && g.AppUserId == user.Id);
            if (result?.GameId != null)
            {
                var coverImage = _webHostEnvironment.WebRootPath + result.BoxImage;
                File.Delete(coverImage);
                _dbContext.Games.Remove(result);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Game> LoadGameGenres(Game game)
        {
            var allGenres = await _dbContext.Genres.ToListAsync();
            var newGenres = game.Genres.Select(g => g.Name);
                            
            var existingGenres = allGenres.Where(g => 
                    newGenres.Contains(g.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();
                            
            var nonExistingGenres = newGenres.Where(ng =>
                    !allGenres.Select(dg => dg.Name).Contains(ng, StringComparer.OrdinalIgnoreCase))
                .ToList();
                            
            existingGenres.AddRange(nonExistingGenres
                .Select(neg => new Genres {Name = neg.Trim().Transform(To.TitleCase)}));

            game.Genres = existingGenres;
            return game;
        }
        
        
        public async Task<Game> Update(Game game)
        {
            // update genre ids if already present in the db
            var existingGenres = await _dbContext.Genres.AsNoTracking().ToListAsync();
            game.Genres.Where(g => g.GenreId == 0).ToList().ForEach(g =>
            {
                g.GenreId = existingGenres.FirstOrDefault(eg => eg.Name == g.Name)?.GenreId ?? 0;
            });
            
            _dbContext.Games.Update(game);
            await _dbContext.SaveChangesAsync();
            
            return game;
        }
        
        public async Task<bool> GameExistsBySteamAppId(long appId, IdentityUser user)
        {
            var game = await _dbContext.Games.FirstOrDefaultAsync(g => 
                g.SteamAppId == appId && g.AppUserId == user.Id);
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

        public async Task<bool> SetCompleteStatusBySteamAppId(uint steamAppId, AppUser user)
        {
            var result = await _dbContext.Games
                .FirstOrDefaultAsync(g => g.SteamAppId == steamAppId && g.AppUserId == user.Id);
            if (result == null) return false;

            return await SetCompleteStatus(result);
        }
        
        public async Task<bool> SetCompleteStatusByGameId(Guid gameId, AppUser user)
        {
            var result = await _dbContext.Games
                .FirstOrDefaultAsync(g => g.GameId == gameId && g.AppUserId == user.Id);
            if (result == null) return false;

            return await SetCompleteStatus(result);
        }
        
        private async Task<bool> SetCompleteStatus(Game result)
        {
            result.Played = !result.Played;
            _dbContext.Games.Update(result);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}