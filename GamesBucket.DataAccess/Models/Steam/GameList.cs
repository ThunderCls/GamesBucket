using System.Text.Json.Serialization;

namespace GamesBucket.DataAccess.Models.Steam
{
    public class GameList
    {
        [JsonPropertyName("response")]
        public Response ApiResponse { get; set; }
        
        public class App
        {
            [JsonPropertyName("appid")]
            public uint Appid { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("last_modified")]
            public uint LastModified { get; set; }

            [JsonPropertyName("price_change_number")]
            public uint PriceChangeNumber { get; set; }
        }

        public class Response
        {
            [JsonPropertyName("apps")]
#nullable enable
            public App[]? Apps { get; set; }
#nullable disable
            [JsonPropertyName("have_more_results")]
            public bool HaveMoreResults { get; set; }

            [JsonPropertyName("last_appid")]
            public uint LastAppid { get; set; }
        }
    }
}