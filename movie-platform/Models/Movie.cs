using Amazon.DynamoDBv2.DataModel;

namespace movie_platform.Models
{
    [DynamoDBTable("Movie")]
    public class Movie
    {
        [DynamoDBHashKey("MovieId")]
        public int MovieId { get; set; }

        [DynamoDBRangeKey("EntryType")]
        public string EntryType { get; set; }  // metadata || rating

        public string Title { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public DateTime ReleaseDate { get; set; }

        [DynamoDBProperty("Rating")]
        public double Rating { get; set; }

        [DynamoDBProperty("Comments")]
        public List<string> Comments { get; set; } = new List<string>();
    }
}
