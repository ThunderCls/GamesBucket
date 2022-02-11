using System;
using System.Net;
using System.Net.Http;
using GamesBucket.App.Services;
using GamesBucket.App.Services.Account;
using GamesBucket.App.Services.EmailService;
using GamesBucket.DataAccess;
using GamesBucket.DataAccess.Models;
using GamesBucket.DataAccess.Services.Api;
using GamesBucket.DataAccess.Services.Api.HLTB;
using GamesBucket.DataAccess.Services.Api.Steam;
using GamesBucket.DataAccess.Services.Games;
using GamesBucket.DataAccess.Services.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GamesBucket.App
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // enforce lowercase routing/query strings in ASP.NET Core
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                //options.LowercaseQueryStrings = true; // don't use with email token verification
            }); 
            
            // global authorization settings
            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            // identity settings
            services.AddIdentity<AppUser, IdentityRole>(options =>
                {
                    options.SignIn.RequireConfirmedEmail = true;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 6;
                }).AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // cookies settings
            services.AddAuthentication(opt =>
            {
                opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromHours(1);
                opt.Cookie.MaxAge = opt.ExpireTimeSpan;
                opt.SlidingExpiration = true;
            });
            
            // tokens lifespan
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(1);
            });
            
            var db = @$"Data Source={_environment.WebRootPath}\db\data.db";
            services.AddDbContext<AppDbContext>(options =>
            {
                //options.UseSqlite(_configuration.GetConnectionString("GamesDb"));
                options.UseSqlite(db, builder =>
                {
                    builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                });
            });

            services.AddAutoMapper(typeof(Startup));
            services.AddSingleton<IEmailService, EmailService>();
            services.AddHttpClient<IHltbService, HltbService>()
                .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                });
            services.AddHttpClient<ISteamService, SteamService>()
                .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                });
            
            services.AddScoped<IGameService, GameService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAccountService, AccountService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseHsts();
            // if (env.IsDevelopment())
            // {
            //     app.UseDeveloperExceptionPage();
            // }
            // else
            // {
            //     app.UseExceptionHandler("/Home/Error");
            //     app.UseHsts();
            // }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}