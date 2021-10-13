using System;
using System.ComponentModel.DataAnnotations;

namespace GamesBucket.App.Models
{
    public class EditGameViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public Guid GameId { get; set; }
        [Required]
        public uint SteamAppId { get; set; }
        [Required]
        public string Name { get; set; }
        public int? MetaCriticScore { get; set; }
        public double? SteamScore { get; set; }
        public DateTime ReleaseDate { get; set; }
        public double GameplayMain { get; set; }
        public double GameplayMainExtra { get; set; }
        public double GameplayCompletionist { get; set; }
    }
}