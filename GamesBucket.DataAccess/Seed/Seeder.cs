using System;
using System.Linq;
using System.Threading.Tasks;
using GamesBucket.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GamesBucket.DataAccess.Seed
{
    public static class Seeder
    {
        public static async Task SeedData(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await SeedRoleData(roleManager);
            
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            await SeedUserData(userManager);
        }

        private static async Task SeedRoleData(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(x => x.Description);
                    throw new ApplicationException(string.Join(".", errors));
                }
            }
            
            if (!await roleManager.RoleExistsAsync(Roles.Guest))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(Roles.Guest));
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(x => x.Description);
                    throw new ApplicationException(string.Join(".", errors));
                }
            }
        }
        
        private static async Task SeedUserData(UserManager<AppUser> userManager)
        {
            foreach (var seedUser in Users.SeedUsers)
            {
                var user = await userManager.FindByEmailAsync(seedUser.Key.Email);
                if (user != null) continue;
                
                seedUser.Key.UserName = seedUser.Key.Email;
                var result = await userManager.CreateAsync(seedUser.Key, seedUser.Value.password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(x => x.Description);
                    throw new ApplicationException(string.Join(".", errors));
                }

                result = await userManager.AddToRoleAsync(seedUser.Key, seedUser.Value.role);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(x => x.Description);
                    throw new ApplicationException(string.Join(".", errors));
                }
            }
        }
    }
}