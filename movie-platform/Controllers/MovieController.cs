using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using movie_platform.Models;

namespace movie_platform.Controllers
{
    public class MovieController : Controller
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly MovieplatformdbContext _context;

        public MovieController(IDynamoDBContext dynamoDbContext, MovieplatformdbContext context)
        {
            _dynamoDbContext = dynamoDbContext;
            _context = context;
        }

        // GET: Movie
        public async Task<IActionResult> Index(string genre, double? minRating)
        {
            // Retrieve all movies
            var movies = await _dynamoDbContext.ScanAsync<Movie>(new List<ScanCondition>()).GetRemainingAsync();

            // Filter by Genre
            if (!string.IsNullOrEmpty(genre))
            {
                movies = movies.Where(m => m.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Filter by Minimum Rating
            if (minRating.HasValue)
            {
                movies = movies.Where(m => m.AverageRating >= minRating.Value).ToList();
            }

            // Store filter values in ViewData to retain them in the view form inputs
            ViewData["CurrentGenre"] = genre;
            ViewData["CurrentMinRating"] = minRating;

            return View(movies);
        }

        // GET: Movie/Add
        [Route("Movie/Add")]
        public IActionResult AddMovie()
        {
            return View();
        }

        // POST: Movie/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Movie/Add")]
        public async Task<IActionResult> AddMovie(Movie movie)
        {
            if (!ModelState.IsValid)
            {
                // Log errors if model state is invalid
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Debug.WriteLine(error.ErrorMessage);
                }
                return View(movie);
            }

            movie.MovieId = new Random().Next(1, 100000);
            movie.EntryType = "Movie";

            try
            {
                await _dynamoDbContext.SaveAsync(movie);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving movie: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Movie/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _dynamoDbContext.LoadAsync<Movie>(id, "Movie");

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _dynamoDbContext.LoadAsync<Movie>(id, "Movie");

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movie/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie)
        {

            if (id != movie.MovieId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // Log errors if model state is invalid
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Debug.WriteLine(error.ErrorMessage);
                }
                return View(movie);
            }

            try
            {
                // retrieve the existing movie
                var existingMovie = await _dynamoDbContext.LoadAsync<Movie>(id, "Movie");
                if (existingMovie == null)
                {
                    return NotFound();
                }

                // Update only the editable fields
                existingMovie.Title = movie.Title;
                existingMovie.Genre = movie.Genre;
                existingMovie.Director = movie.Director;
                existingMovie.ReleaseYear = movie.ReleaseYear;

                await _dynamoDbContext.SaveAsync(existingMovie);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving movie: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Movie/AddRate/34663
        [HttpGet]
        [Route("Movie/AddRate/{movieId}")]
        public async Task<IActionResult> AddRate(int movieId)
        {
            var movie = await _dynamoDbContext.LoadAsync<Movie>(movieId, "Movie");

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Movie/AddRate/{movieId}")]
        public async Task<IActionResult> AddRate(int movieId, int rate)
        {
            //get username from session
            var username = HttpContext.Session.GetString("UserName");
            Debug.WriteLine($"User {username} is rating movie {movieId} with {rate}");

            // get userId based on username
            var userId = await _context.Users
                .Where(u => u.UserName.ToLower() == username.ToLower()) // Compare in lowercase
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (userId == 0)
            {
                ModelState.AddModelError("", "User not logged in.");
                return RedirectToAction("AddRate", new { movieId }); // back to the rating
            }

            var newRating = new Rating
            {
                MovieId = movieId,
                EntryType = "Rating",
                UserId = (int)userId,
                Rate = rate,
                CreatedAt = DateTime.UtcNow
            };

            // movie based on movieId
            var movie = await _dynamoDbContext.LoadAsync<Movie>(movieId, "Movie");

            // Add the rating to the movie
            movie.Ratings.Add(rate);
            movie.AverageRating = movie.Ratings.Count > 0 ? movie.Ratings.Average() : 0;

            // Save the rating to the database
            await _dynamoDbContext.SaveAsync(movie);

            return RedirectToAction(nameof(Index));
        }

        // GET: Movie/Comments/34663
        [HttpGet]
        [Route("Movie/Comments/{movieId}")]
        public async Task<IActionResult> Comments(int movieId)
        {
            var movie = await _dynamoDbContext.LoadAsync<Movie>(movieId, "Movie");

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movie/AddComment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Movie/AddComment/5")]
        public async Task<IActionResult> AddComment(int movieId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "Comment cannot be empty.");
                return RedirectToAction("Comments", new { movieId }); // back to the comments
            }

            //get username from session
            var username = HttpContext.Session.GetString("UserName");
            Debug.WriteLine($"User {username} is adding a comment to movie {movieId}");

            // get userId based on username
            var userId = await _context.Users
                .Where(u => u.UserName.ToLower() == username.ToLower()) // Compare in lowercase
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (userId == 0)
            {
                ModelState.AddModelError("", "User not logged in.");
                return RedirectToAction("Comments", new { movieId }); // back to the comments
            }

            var newComment = new Comment
            {
                MovieId = movieId,
                EntryType = "Comment",
                CommentId = new Random().Next(1, 100000),
                UserId = (int)userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            // pega o movie based on movieId
            var movie = await _dynamoDbContext.LoadAsync<Movie>(movieId, "Movie");

            // Add the comment to the movie
            movie.Comments.Add(content);

            // Save the comment to the database
            await _dynamoDbContext.SaveAsync(movie);

            return RedirectToAction("Comments", new { movieId }); 
        }
    }
}
