using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace movie_platform.Models
{
    [DynamoDBTable("Movie")]
    public class Movie
    {
        // Partition key
        [DynamoDBHashKey("MovieId")]
        public int MovieId { get; set; }

        // Sort key
        [DynamoDBRangeKey("EntryType")]
        public string EntryType { get; set; } = "Movie"; // default

        // Movie-specific attributes
        public string Title { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public int ReleaseYear { get; set; }
        public List<int> Ratings { get; set; } = new List<int>();
        public List<string> Comments { get; set; } = new List<string>();

        // Calculate average for GSI
        [DynamoDBProperty("AverageRating")]
        public double AverageRating { get; set; }
    }

    [DynamoDBTable("Movie")]
    public class Rating
    {
        [DynamoDBHashKey("MovieId")]
        public int MovieId { get; set; }

        [DynamoDBRangeKey("EntryType")]
        public string EntryType { get; set; } = "Rating";

        public int UserId { get; set; }
        public int Rate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    [DynamoDBTable("Movie")]
    public class Comment
    {
        [DynamoDBHashKey("MovieId")]
        public int MovieId { get; set; }

        [DynamoDBRangeKey("EntryType")]
        public string EntryType { get; set; } = "Comment";

        public int CommentId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
