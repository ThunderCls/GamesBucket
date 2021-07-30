using System.Collections.Generic;

namespace GamesBucket.DataAccess.Models
{
    public class Genres
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Game> Games { get; set; }
    }
}
