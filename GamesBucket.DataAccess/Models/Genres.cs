using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GamesBucket.DataAccess.Models
{
    public class Genres
    {
        [Key]
        public int GenreId { get; set; }
        public string Name { get; set; }
        public ICollection<Game> Games { get; set; }
    }
}
