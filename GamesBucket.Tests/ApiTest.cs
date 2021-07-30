using System.Collections.Generic;
using System.Threading.Tasks;
using GamesBucket.DataAccess;
using GamesBucket.DataAccess.Models;
using GamesBucket.DataAccess.Services.ApiService;
using GamesBucket.DataAccess.Services.ApiService.HLTB;
using GamesBucket.DataAccess.Services.ApiService.Steam;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace GamesBucket.Tests
{
    public class ApiTest
    {
        private IConfiguration _configuration;
        private IApiService _apiService;
        
        [SetUp]
        public void Setup()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"SteamApi:Key", "3930336732F2DEEF008D1F18A533F944"},
            };
            
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
            
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "LibraryInfo")
                .Options;

            _apiService = new SteamService(_configuration, new AppDbContext(options));
        }

        [Test]
        public async Task SearchGameListByTitle()
        {
            var result = await _apiService.SearchGameByTitle("Portal");
            Assert.IsNotEmpty(result);
        }

        [Test]
        public async Task Can_Parse_Json()
        {
            // 22120, 22140, 22180
            var app = new GamesBucket.DataAccess.Services.ApiService.Steam.GameList.App {Appid = 400};
            Assert.IsNotNull(await _apiService.GetGameInfo(app));
        }

        [Test]
        public void Can_Update_Steam_Library_Db()
        {
            Assert.DoesNotThrowAsync(() => _apiService.UpdateSteamLibrary());
        }

        [Test]
        public async Task Can_Grab_Image_Url()
        {
            var hltbService = new HltbService();
            var game = "STAR WARS™ Jedi Knight - Mysteries of the Sith™";
            Assert.IsNotNull((await hltbService.GetHltbGameInfo(new Game { Name = game})).BoxImageHLTB);
        }
    }
}