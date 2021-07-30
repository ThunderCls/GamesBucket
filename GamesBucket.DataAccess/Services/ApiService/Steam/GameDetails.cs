using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable enable
namespace GamesBucket.DataAccess.Services.ApiService.Steam
{
    public class GameDetails
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public Data GameData { get; set; } = new ();

        public class Data
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("steam_appid")]
            public uint SteamAppid { get; set; }

            [JsonPropertyName("required_age")]
            public object RequiredAge { get; set; }

            [JsonPropertyName("is_free")]
            public bool IsFree { get; set; }

            [JsonPropertyName("dlc")]
            public List<int> Dlc { get; set; } = new ();

            [JsonPropertyName("detailed_description")]
            public string DetailedDescription { get; set; }

            [JsonPropertyName("about_the_game")]
            public string AboutTheGame { get; set; }

            [JsonPropertyName("short_description")]
            public string ShortDescription { get; set; }

            [JsonPropertyName("supported_languages")]
            public string SupportedLanguages { get; set; }

            [JsonPropertyName("header_image")]
            public string HeaderImage { get; set; }

            [JsonPropertyName("website")]
            public string Website { get; set; }

            [JsonPropertyName("developers")]
            public List<string> Developers { get; set; } = new ();

            [JsonPropertyName("publishers")]
            public List<string> Publishers { get; set; } = new ();

            [JsonPropertyName("price_overview")]
            public PriceOverview PriceOverview { get; set; } = new ();

            [JsonPropertyName("packages")]
            public List<int> Packages { get; set; } = new ();

            [JsonPropertyName("platforms")]
            public Platforms Platforms { get; set; } = new ();

            [JsonPropertyName("metacritic")]
            public Metacritic Metacritic { get; set; } = new ();

            [JsonPropertyName("categories")]
            public List<Category> Categories { get; set; } = new ();

            [JsonPropertyName("genres")]
            public List<Genre> Genres { get; set; } = new ();

            [JsonPropertyName("screenshots")]
            public List<Screenshot> Screenshots { get; set; } = new ();

            [JsonPropertyName("movies")]
            public List<Movie> Movies { get; set; } = new ();

            [JsonPropertyName("recommendations")]
            public Recommendations Recommendations { get; set; } = new ();

            [JsonPropertyName("achievements")]
            public Achievements Achievements { get; set; } = new ();

            [JsonPropertyName("release_date")]
            public ReleaseDate ReleaseDate { get; set; } = new ();

            [JsonPropertyName("support_info")]
            public SupportInfo? SupportInfo { get; set; }

            [JsonPropertyName("background")]
            public string Background { get; set; }
        }
        
        public class PriceOverview
        {
            [JsonPropertyName("currency")]
            public string Currency { get; set; }

            [JsonPropertyName("initial")]
            public int Initial { get; set; }

            [JsonPropertyName("final")]
            public int Final { get; set; }

            [JsonPropertyName("discount_percent")]
            public int DiscountPercent { get; set; }

            [JsonPropertyName("initial_formatted")]
            public string InitialFormatted { get; set; }

            [JsonPropertyName("final_formatted")]
            public string FinalFormatted { get; set; }
        }

        public class Platforms
        {
            [JsonPropertyName("windows")]
            public bool Windows { get; set; }

            [JsonPropertyName("mac")]
            public bool Mac { get; set; }

            [JsonPropertyName("linux")]
            public bool Linux { get; set; }
        }

        public class Metacritic
        {
            [JsonPropertyName("score")]
            public int Score { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }
        }
        
        public class Recommendations
        {
            [JsonPropertyName("total")]
            public int Total { get; set; }
        }

        public class Achievements
        {
            [JsonPropertyName("total")]
            public int Total { get; set; }

            [JsonPropertyName("highlighted")]
            public List<Highlighted> Highlighted { get; set; } = new ();
        }
        
        public class Highlighted
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("path")]
            public string Path { get; set; }
        }
        
        public class ReleaseDate
        {
            [JsonPropertyName("coming_soon")]
            public bool ComingSoon { get; set; }

            [JsonPropertyName("date")]
            public string Date { get; set; }
        }
        
        public class SupportInfo
        {
            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }
        }

        public class Category
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }
        }

        public class Genre
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }
        }

        public class Screenshot
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("path_thumbnail")]
            public string PathThumbnail { get; set; }

            [JsonPropertyName("path_full")]
            public string PathFull { get; set; }
        }
        
        public class Movie
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("thumbnail")]
            public string Thumbnail { get; set; }

            [JsonPropertyName("webm")]
            public Webm Webm { get; set; } = new ();

            [JsonPropertyName("mp4")]
            public Mp4 Mp4 { get; set; } = new ();

            [JsonPropertyName("highlight")]
            public bool Highlight { get; set; }
        }
        
        public class Webm
        {
            [JsonPropertyName("480")]
            public string _480 { get; set; }

            [JsonPropertyName("max")]
            public string Max { get; set; }
        }

        public class Mp4
        {
            [JsonPropertyName("480")]
            public string _480 { get; set; }

            [JsonPropertyName("max")]
            public string Max { get; set; }
        }
    }
}