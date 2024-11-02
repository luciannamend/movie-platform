using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using movie_platform.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace movie_platform.Services
{
    public class DynamoDBMovieOperation
    {
        private readonly IAmazonDynamoDB _client;
        private readonly DynamoDBContext _context;

        public DynamoDBMovieOperation(IAmazonDynamoDB client)
        {
            _client = client;
            _context = new DynamoDBContext(_client);
        }

        // Method to create the "Movie" table
        public async Task CreateMovieTableWithIndexesAsync()
        {
            var request = new CreateTableRequest
            {
                TableName = "Movie",
                // Attributes
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("MovieId", ScalarAttributeType.N),
                    new AttributeDefinition("EntryType", ScalarAttributeType.S),
                    new AttributeDefinition("Genre", ScalarAttributeType.S),
                    new AttributeDefinition("AverageRating", ScalarAttributeType.N)
                },
                // Primary key
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("MovieId", KeyType.HASH),   // Partition key
                    new KeySchemaElement("EntryType", KeyType.RANGE) // Sort key
                },

                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                },

                // Secondary indexes for searching 
                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                {
                    // by genre
                    new GlobalSecondaryIndex
                    {
                        IndexName = "GenreIndex",
                        KeySchema = new List<KeySchemaElement>
                        {
                            new KeySchemaElement("Genre", KeyType.HASH)
                        },
                        Projection = new Projection
                        {
                            ProjectionType = ProjectionType.ALL
                        },
                        ProvisionedThroughput = new ProvisionedThroughput
                        {
                            ReadCapacityUnits = 5,
                            WriteCapacityUnits = 5
                        }
                    },
                    // by rate average
                    new GlobalSecondaryIndex
                    {
                        IndexName = "AverageRatingIndex",
                        KeySchema = new List<KeySchemaElement>
                        {
                            new KeySchemaElement("AverageRating", KeyType.HASH)
                        },
                        Projection = new Projection
                        {
                            ProjectionType = ProjectionType.ALL
                        },
                        ProvisionedThroughput = new ProvisionedThroughput
                        {
                            ReadCapacityUnits = 5,
                            WriteCapacityUnits = 5
                        }
                    }
                }
            };
            // Check if table already exists
            var existingTables = await _client.ListTablesAsync();

            // If doesn't exist, create it
            if (!existingTables.TableNames.Contains("Movie"))
            {
                await _client.CreateTableAsync(request);
                Debug.WriteLine("Movie tb created.");

                // Wait until the table is created
                bool isTableAvailable = false;
                while (!isTableAvailable)
                {
                    var tableStatus = await _client.DescribeTableAsync("Movie");
                    isTableAvailable = tableStatus.Table.TableStatus == "ACTIVE";
                    await Task.Delay(5000);
                }

                // Add a few sample movies
                await AddSampleMovie();
            }
            else
            {
                Debug.WriteLine("Table 'Movie' already exists.");
            }
        }        

        // Method to add 3 movies
        public async Task AddSampleMovie()
        {
            var movies = new List<Movie>
            {
                new Movie
                {
                    MovieId = 1,
                    Title = "A Quiet Place",
                    Genre = "Horror",
                    Director = "John Krasinski",
                    ReleaseYear = 2018,
                    Ratings = new List<int> { 8, 9 },
                    Comments = new List<string> { "Very thrilling!", "Loved the suspense." }
                },
                new Movie
                {
                    MovieId = 2,
                    Title = "Titanic",
                    Genre = "Drama",
                    Director = "James Cameron",
                    ReleaseYear = 1997,
                    Ratings = new List<int> { 10, 9 },
                    Comments = new List<string> { "A classic!", "Beautiful and tragic." }
                },
                new Movie
                {
                    MovieId = 3,
                    Title = "Snow White",
                    Genre = "Fantasy",
                    Director = "David Hand",
                    ReleaseYear = 2025,
                    Ratings = new List<int> { 7, 8 },
                    Comments = new List<string> { "Timeless story.", "Great for all ages." }
                }
            };

            foreach (var movie in movies)
            {
                movie.EntryType = "Movie";
                movie.AverageRating = movie.Ratings.Count > 0 ? movie.Ratings.Average() : 0;
                await _context.SaveAsync(movie);
            }

            Console.WriteLine("Sample movies added to DynamoDB.");
        }

        // Method to retrieve a movie by its ID and entry type
        public async Task<Movie> GetMovieAsync(int movieId, string entryType)
        {
            try
            {
                var movie = await _context.LoadAsync<Movie>(movieId, entryType);
                return movie;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving movie: {ex.Message}");
                return null;
            }
        }

        // Method to get all movies
        public async Task<List<Movie>> GetAllMoviesAsync()
        {
            var scanConditions = new List<ScanCondition>();
            var search = _context.ScanAsync<Movie>(scanConditions);

            try
            {
                var movies = await search.GetNextSetAsync();
                return movies;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving movies: {ex.Message}");
                return null;
            }
        }
    }
}
