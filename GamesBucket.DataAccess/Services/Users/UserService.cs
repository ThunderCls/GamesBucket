using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GamesBucket.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GamesBucket.DataAccess.Services.Users
{
    public class UserService : ControllerBase, IUserService
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;

        public UserService(AppDbContext dbContext, UserManager<AppUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }
        
        public async Task<AppUser> GetUserById(string id)
        {
            return await _dbContext.Users.FindAsync(id);
        }

        public async Task<AppUser> AddUser(AppUser newUser)
        {
            await _dbContext.Users.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();
            return newUser;
        }

        public async Task<bool> RemoveUser(string id)
        {
            var user = await GetUserById(id);
            if (user == null) return false;

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<IdentityError>> UpdateUser(AppUser updatedUser)
        {
            var result = await _userManager.UpdateAsync(updatedUser);
            return result.Errors;
        }
        
        public async Task<AppUser> GetLoggedUser(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return null;
            
            var loggedUser = await _userManager.FindByIdAsync(userId);
            if (loggedUser == null) return null;

            return loggedUser;
        }
    }
}