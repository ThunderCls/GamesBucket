using System;
using System.Collections.Generic;
using GamesBucket.DataAccess.Services.ApiService.Steam;

namespace GamesBucket.DataAccess.Models.Dtos
{
    public class SearchResult
    {
        public Guid GameId { get; set; }
        public long SteamAppId { get; set; }
        public string Name { get; set; }
        public GameDetails.Platforms Platforms { get; set; }
        public string BoxImage { get; set; }
        public string HeaderImage { get; set; }
        public List<Genres> Genres { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}