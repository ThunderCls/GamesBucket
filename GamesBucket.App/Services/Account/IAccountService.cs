using System.Threading.Tasks;
using GamesBucket.DataAccess.Models;
using Microsoft.AspNetCore.Http;

namespace GamesBucket.App.Services.Account
{
    public interface IAccountService
    {
        Task ResendConfirmationEmail(string email, HttpContext httpContext);
        Task RequestConfirmationEmail(AppUser userIdentity, HttpContext httpContext);
        Task RequestPasswordReset(AppUser user, HttpContext httpContext);
    }
}