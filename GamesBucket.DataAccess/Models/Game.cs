using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GamesBucket.DataAccess.Models
{
    [Index(nameof(SteamAppId))]
    public class Game
    {
        [Key]
        public int Id { get; set; }
        public uint SteamAppId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DetailedDescription { get; set; }
        public string SupportedLanguages { get; set; }
        public string AboutTheGame { get; set; }
        public string ShortDescription { get; set; }
        public string HeaderImage { get; set; }
        public string BoxImage { get; set; }
        public string BoxImageHLTB { get; set; }
        public string BackgroundImage { get; set; }
        public string Website { get; set; }
        public int? MetaCriticScore { get; set; }
        public string MetaCriticUrl { get; set; }
        public double? SteamScore { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Developers { get; set; }
        public bool Linux { get; set; }
        public bool Mac { get; set; }
        public bool Windows { get; set; }
        public List<Genres> Genres { get; set; }
        public List<Screenshots> Screenshots { get; set; }
        public List<Movies> Movies { get; set; }
        public double GameplayMain { get; set; }
        public double GameplayMainExtra { get; set; }
        public double GameplayCompletionist { get; set; }
    }
}