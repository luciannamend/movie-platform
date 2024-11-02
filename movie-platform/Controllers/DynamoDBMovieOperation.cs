using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using movie_platform.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace movie_platform.Controllers
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

                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("MovieId", KeyType.HASH),   // Partition key
                    new KeySchemaElement("EntryType", KeyType.RANGE) // Sort key
                },

                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("MovieId", ScalarAttributeType.N),
                    new AttributeDefinition("EntryType", ScalarAttributeType.S),
                    new AttributeDefinition("Rating", ScalarAttributeType.N),
                    new AttributeDefinition("Genre", ScalarAttributeType.S)
                },

                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 5
                },

                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                {
                    // GSI for Rating
                    new GlobalSecondaryIndex
                    {
                        IndexName = "RatingIndex",
                        KeySchema = new List<KeySchemaElement>
                        {
                            new KeySchemaElement("Rating", KeyType.HASH),
                            new KeySchemaElement("MovieId", KeyType.RANGE)
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

                    // GSI for Genre
                    new GlobalSecondaryIndex
                    {
                        IndexName = "GenreIndex",

                        KeySchema = new List<KeySchemaElement>
                        {
                            new KeySchemaElement("Genre", KeyType.HASH),
                            new KeySchemaElement("MovieId", KeyType.RANGE)
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

            var existingTables = await _client.ListTablesAsync();
            if (!existingTables.TableNames.Contains("Movie"))
            {
                await _client.CreateTableAsync(request);
                Debug.WriteLine("Table 'Movie' with GSIs created successfully.");

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



        // Method to add a new movie
        public async Task AddMovieAsync(int movieId, string entryType, string title, string genre)
        {
            var movie = new Movie
            {
                MovieId = movieId,
                EntryType = entryType,
                Title = title,
                Genre = genre
            };

            try
            {
                await _context.SaveAsync(movie);
                Debug.WriteLine($"Movie '{title}' added successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding movie: {ex.Message}");
            }
        }

        // add 3 sample movies
        public async Task AddSampleMovie()
        {
            List<Movie> movies = new List<Movie>();

            var aQuitePlace = new Movie
            {
                MovieId = 1,
                EntryType = "metadata",
                Title = "A Quite Place",
                Genre = "Horror",
                Director = "John Krasinski",
                ReleaseYear = 2018
            };
            movies.Add(aQuitePlace);

            var titanic = new Movie
            {
                MovieId = 2,
                EntryType = "metadata",
                Title = "Titanic",
                Genre = "Drama",
                Director = "James Cameron",
                ReleaseYear = 1997
            };
            movies.Add(titanic);

            var snowWhite = new Movie
            {
                MovieId = 3,
                EntryType = "metadata",
                Title = "Snow White",
                Genre = "Fantasy",
                Director = "Marc Webb",
                ReleaseYear = 2025
            };
            movies.Add(snowWhite);

            foreach (var movie in movies)
            {
                await _context.SaveAsync(movie);
            }
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
