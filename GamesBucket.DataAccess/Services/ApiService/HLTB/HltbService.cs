using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GamesBucket.DataAccess.Models;
using HtmlAgilityPack;

namespace GamesBucket.DataAccess.Services.ApiService.HLTB
{
    public class HltbService
    {
        private readonly HttpClient _httpClient;

        private const string GameBeatTimeEndpoint = "https://howlongtobeat.com/search_results?";

        public HltbService()
        {
            var handler = new HttpClientHandler() { 
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            
            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", 
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "*/*");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://howlongtobeat.com");
        }

        // https://github.com/ckatzorke/howlongtobeat
        public async Task<Game> GetHltbGameInfo(Game game)
        {
            //try
            {
                var cleanedName = Regex.Replace(game.Name, @"[^a-zA-Z0-9()\s]", string.Empty);
                var content = new StringContent($"queryString={cleanedName}&t=games&sorthead=popular&sortd=0&plat=&" + 
                                                "length_type=main&length_min=&length_max=&v=&f=&g=&detail=&randomize=0", 
                    Encoding.UTF8, "application/x-www-form-urlencoded");

                var url = $"{GameBeatTimeEndpoint}page=1";
                var result = await _httpClient.PostAsync(url, content);
                if (!result.IsSuccessStatusCode) return game;

                var htmlResult = await result.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(htmlResult)) return game;

                // if game not found return
                // NOTE: if game not returned in the first results it might not exist (lazy I know :8)
                if (htmlResult.IndexOf("<img alt=\"Box Art\"", 
                    StringComparison.OrdinalIgnoreCase) < 0) return game;
                
                // parse html
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlResult);

                // NOTE: getting values from the first result (lazy I know :8)
                // Main Story
                var main = htmlDoc.DocumentNode.SelectSingleNode("//ul/li[1]/div[2]/div/div/div[2]")?.InnerText;
                // Main + Extra
                var extra = htmlDoc.DocumentNode.SelectSingleNode("//ul/li[1]/div[2]/div/div/div[4]")?.InnerText;
                // Completionist
                var complete = htmlDoc.DocumentNode.SelectSingleNode("//ul/li[1]/div[2]/div/div/div[6]")?.InnerText;

                var node = htmlDoc.DocumentNode.SelectSingleNode(
                    "//ul/li[1]/div[1]/a[1]/img[1]");
                game.GameplayMain = HtmlStringToDouble(main);
                game.GameplayMainExtra = HtmlStringToDouble(extra);
                game.GameplayCompletionist = HtmlStringToDouble(complete);
                game.BoxImageHLTB = "https://howlongtobeat.com" + node.GetAttributeValue("src", String.Empty);
            }
            //catch { }

            return game;
        }

        private double HtmlStringToDouble(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            
            var wholeNumber = text.ToCharArray().TakeWhile(char.IsNumber).ToList();
            double decimalPortion = 0;
            if (text.Contains("&#189;")) decimalPortion = 0.5;
            if (double.TryParse(string.Join("", wholeNumber), out var number)) 
                return number + decimalPortion;

            return 0;
        }
    }
}