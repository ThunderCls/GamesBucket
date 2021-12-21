using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable enable
namespace GamesBucket.DataAccess.Models.Steam
{
    public class GameDetails
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public Data GameData { get; set; } = new Data();

        public class Data
        {
            [JsonPropertyName("type")]
            public string? Type { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("steam_appid")]
            public uint SteamAppid { get; set; }

            [JsonPropertyName("required_age")]
            public object? RequiredAge { get; set; }

            [JsonPropertyName("is_free")]
            public bool IsFree { get; set; }

            [JsonPropertyName("dlc")]
            public List<int> Dlc { get; set; } = new List<int>();

            [JsonPropertyName("detailed_description")]
            public string? DetailedDescription { get; set; }

            [JsonPropertyName("about_the_game")]
            public string? AboutTheGame { get; set; }

            [JsonPropertyName("short_description")]
            public string? ShortDescription { get; set; }

            [JsonPropertyName("supported_languages")]
            public string? SupportedLanguages { get; set; }

            [JsonPropertyName("header_image")]
            public string? HeaderImage { get; set; }

            [JsonPropertyName("website")]
            public string? Website { get; set; }

            [JsonPropertyName("developers")]
            public List<string> Developers { get; set; } = new List<string>();

            [JsonPropertyName("publishers")]
            public List<string> Publishers { get; set; } = new List<string>();

            [JsonPropertyName("price_overview")]
            public PriceOverview PriceOverview { get; set; } = new PriceOverview();

            [JsonPropertyName("packages")]
            public List<int> Packages { get; set; } = new List<int>();

            [JsonPropertyName("platforms")]
            public Platforms Platforms { get; set; } = new Platforms();

            [JsonPropertyName("metacritic")]
            public Metacritic Metacritic { get; set; } = new Metacritic();

            [JsonPropertyName("categories")]
            public List<Category> Categories { get; set; } = new List<Category>();

            [JsonPropertyName("genres")]
            public List<Genre> Genres { get; set; } = new List<Genre>();

            [JsonPropertyName("screenshots")]
            public List<Screenshot> Screenshots { get; set; } = new List<Screenshot>();

            [JsonPropertyName("movies")]
            public List<Movie> Movies { get; set; } = new List<Movie>();

            [JsonPropertyName("recommendations")]
            public Recommendations Recommendations { get; set; } = new Recommendations();

            [JsonPropertyName("achievements")]
            public Achievements Achievements { get; set; } = new Achievements();

            [JsonPropertyName("release_date")]
            public ReleaseDate ReleaseDate { get; set; } = new ReleaseDate();

            [JsonPropertyName("support_info")]
            public SupportInfo? SupportInfo { get; set; }

            [JsonPropertyName("background")]
            public string? Background { get; set; }
        }
        
        public class PriceOverview
        {
            [JsonPropertyName("currency")]
            public string? Currency { get; set; }

            [JsonPropertyName("initial")]
            public int Initial { get; set; }

            [JsonPropertyName("final")]
            public int Final { get; set; }

            [JsonPropertyName("discount_percent")]
            public int DiscountPercent { get; set; }

            [JsonPropertyName("initial_formatted")]
            public string? InitialFormatted { get; set; }

            [JsonPropertyName("final_formatted")]
            public string? FinalFormatted { get; set; }
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
            public string? Url { get; set; }
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
            public List<Highlighted> Highlighted { get; set; } = new List<Highlighted>();
        }
        
        public class Highlighted
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("path")]
            public string? Path { get; set; }
        }
        
        public class ReleaseDate
        {
            [JsonPropertyName("coming_soon")]
            public bool ComingSoon { get; set; }

            [JsonPropertyName("date")]
            public string? Date { get; set; }
        }
        
        public class SupportInfo
        {
            [JsonPropertyName("url")]
            public string? Url { get; set; }

            [JsonPropertyName("email")]
            public string? Email { get; set; }
        }

        public class Category
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }
        }

        public class Genre
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }
        }

        public class Screenshot
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("path_thumbnail")]
            public string? PathThumbnail { get; set; }

            [JsonPropertyName("path_full")]
            public string? PathFull { get; set; }
        }
        
        public class Movie
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("thumbnail")]
            public string? Thumbnail { get; set; }

            [JsonPropertyName("webm")]
            public Webm Webm { get; set; } = new Webm();

            [JsonPropertyName("mp4")]
            public Mp4 Mp4 { get; set; } = new Mp4();

            [JsonPropertyName("highlight")]
            public bool Highlight { get; set; }
        }
        
        public class Webm
        {
            [JsonPropertyName("480")]
            public string? _480 { get; set; }

            [JsonPropertyName("max")]
            public string? Max { get; set; }
        }

        public class Mp4
        {
            [JsonPropertyName("480")]
            public string? _480 { get; set; }

            [JsonPropertyName("max")]
            public string? Max { get; set; }
        }
    }
}