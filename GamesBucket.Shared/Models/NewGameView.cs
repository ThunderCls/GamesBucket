using System;
using Microsoft.AspNetCore.Http;

namespace GamesBucket.Shared.Models
{
    public class NewGameView
    {
        public string Name { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Developers { get; set; }
        public string Genres { get; set; }
        public string SupportedLanguages { get; set; }
        public int? MetaCriticScore { get; set; }
        public double? SteamScore { get; set; }
        public bool Linux { get; set; }
        public bool Mac { get; set; }
        public bool Windows { get; set; }
        public double GameplayMain { get; set; }
        public double GameplayMainExtra { get; set; }
        public double GameplayCompletionist { get; set; }
        public string ShortDescription { get; set; }
        public string BoxImage { get; set; }
        public IFormFile CoverPhoto { get; set; }
    }
}