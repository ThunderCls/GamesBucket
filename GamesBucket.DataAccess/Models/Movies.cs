using System.ComponentModel.DataAnnotations;

namespace GamesBucket.DataAccess.Models
{
    public class Movies
    {
        [Key]
        public int MovieId { get; set; }
        public int GameId { get; set; }
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public string Webm { get; set; }
    }
}
