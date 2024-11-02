using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace movie_platform.Models
{
    [DynamoDBTable("Movie")]
    public class Movie
    {
        [DynamoDBHashKey("MovieId")]
        public int MovieId { get; set; }

        [DynamoDBRangeKey("EntryType")]
        public string EntryType { get; set; }

        // Metadata
        public string Title { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public int ReleaseYear { get; set; }

        // Ratings
        [DynamoDBProperty("Ratings")]
        public List<int> Ratings { get; set; } = new List<int>();

        // Comments as a nested complex type
        [DynamoDBProperty("Comments")]
        public List<string> Comments { get; set; } = new List<string>();
    }

    //public class Rating
    //{
    //    public int Rate { get; set; }

    //    public int UserId { get; set; }
    //}

    //public class Comment
    //{
    //    public string TextComment { get; set; }

    //    public int UserId { get; set; }

    //    public DateTime CreatedAt { get; set; }
    //}
}
