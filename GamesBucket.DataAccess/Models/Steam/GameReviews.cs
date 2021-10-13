using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GamesBucket.DataAccess.Models.Steam
{
    public class GameReviews
    {
        [JsonPropertyName("success")]
        public long Success { get; set; }

        [JsonPropertyName("query_summary")]
        public QuerySummary Summary { get; set; }

        [JsonPropertyName("reviews")]
        public List<Review> Reviews { get; set; }

        [JsonPropertyName("cursor")]
        public string Cursor { get; set; }
        
        public class QuerySummary
        {
            [JsonPropertyName("num_reviews")]
            public long NumReviews { get; set; }

            [JsonPropertyName("review_score")]
            public long ReviewScore { get; set; }

            [JsonPropertyName("review_score_desc")]
            public string ReviewScoreDesc { get; set; }

            [JsonPropertyName("total_positive")]
            public long TotalPositive { get; set; }

            [JsonPropertyName("total_negative")]
            public long TotalNegative { get; set; }

            [JsonPropertyName("total_reviews")]
            public long TotalReviews { get; set; }
        }

        public class Review
        {
            [JsonPropertyName("recommendationid")]
            public string Recommendationid { get; set; }

            [JsonPropertyName("author")]
            public Author Author { get; set; }

            [JsonPropertyName("language")]
            public string Language { get; set; }

            [JsonPropertyName("review")]
            public string ReviewReview { get; set; }

            [JsonPropertyName("timestamp_created")]
            public long TimestampCreated { get; set; }

            [JsonPropertyName("timestamp_updated")]
            public long TimestampUpdated { get; set; }

            [JsonPropertyName("voted_up")]
            public bool VotedUp { get; set; }

            [JsonPropertyName("votes_up")]
            public long VotesUp { get; set; }

            [JsonPropertyName("votes_funny")]
            public long VotesFunny { get; set; }

            [JsonPropertyName("weighted_vote_score")]
            public string WeightedVoteScore { get; set; }

            [JsonPropertyName("comment_count")]
            public long CommentCount { get; set; }

            [JsonPropertyName("steam_purchase")]
            public bool SteamPurchase { get; set; }

            [JsonPropertyName("received_for_free")]
            public bool ReceivedForFree { get; set; }

            [JsonPropertyName("written_during_early_access")]
            public bool WrittenDuringEarlyAccess { get; set; }
        }

        public class Author
        {
            [JsonPropertyName("steamid")]
            public string Steamid { get; set; }

            [JsonPropertyName("num_games_owned")]
            public long NumGamesOwned { get; set; }

            [JsonPropertyName("num_reviews")]
            public long NumReviews { get; set; }

            [JsonPropertyName("playtime_forever")]
            public long PlaytimeForever { get; set; }

            [JsonPropertyName("playtime_last_two_weeks")]
            public long PlaytimeLastTwoWeeks { get; set; }

            [JsonPropertyName("playtime_at_review")]
            public long PlaytimeAtReview { get; set; }

            [JsonPropertyName("last_played")]
            public long LastPlayed { get; set; }
        }
    }
}