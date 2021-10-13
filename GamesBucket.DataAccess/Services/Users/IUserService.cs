using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GamesBucket.DataAccess.Models;
using Microsoft.AspNetCore.Identity;

namespace GamesBucket.DataAccess.Services.Users
{
    public interface IUserService
    {
        Task<AppUser> GetLoggedUser(ClaimsPrincipal user);
        Task<AppUser> GetUserById(string id);
        Task<AppUser> AddUser(AppUser newUser);
        Task<bool> RemoveUser(string id);
        Task<IEnumerable<IdentityError>> UpdateUser(AppUser updatedUser);
    }
}