using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GamesBucket.DataAccess.Models
{
    [Index(nameof(Name))]
    public class SteamLibrary
    {
        [Key]
        public uint SteamAppId { get; set; }
        public string Name { get; set; }
    }
}