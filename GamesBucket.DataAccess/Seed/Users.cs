using System.Collections.Generic;
using GamesBucket.DataAccess.Models;

namespace GamesBucket.DataAccess.Seed
{
    public static class Users
    {
        public static Dictionary<AppUser, (string password, string role)> SeedUsers => new Dictionary<AppUser, (string password, string role)>()
        {
            {
                new AppUser
                {
                    Email = "yunietps@gmail.com",
                    EmailConfirmed = true,
                    FirstName = "Yuniet",
                    LastName = "Piloto"
                },
                (password: "admin123", role: Roles.Admin)
            },
            {
                new AppUser
                {
                    Email = "guest@mail.com",
                    EmailConfirmed = true,
                    FirstName = "Guest",
                    LastName = "Demo"
                },
                (password: "guest123", role: Roles.Guest)
            }
        };
    }
}