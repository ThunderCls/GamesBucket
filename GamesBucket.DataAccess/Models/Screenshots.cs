using System.ComponentModel.DataAnnotations;

namespace GamesBucket.DataAccess.Models
{
    public class Screenshots
    {
        [Key]
        public int ScreenshotId { get; set; }
        public int GameId { get; set; }
        public string PathThumbnail { get; set; }
        public string PathFull { get; set; }
    }
}
