using GamesBucket.DataAccess;
using GamesBucket.DataAccess.Services.ApiService;
using GamesBucket.DataAccess.Services.ApiService.Steam;
using GamesBucket.DataAccess.Services.GameService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            // NOTE: enforce lowercase routing/query strings in ASP.NET Core
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                //options.LowercaseQueryStrings = true; // don't use with email token verification
            }); 
            
            services.AddControllersWithViews();

            var db = @$"Data Source={_environment.WebRootPath}\db\data.db";
            services.AddDbContext<AppDbContext>(options =>
            {
                //options.UseSqlite(_configuration.GetConnectionString("GamesDb"));
                options.UseSqlite(db);
            });

            services.AddAutoMapper(typeof(Startup));
            services.AddTransient<IGameService, GameService>();
            services.AddTransient<IApiService, SteamService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}