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

        public MovieController(IDynamoDBContext dynamoDbContext)
        {
            _dynamoDbContext = dynamoDbContext;
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
                movies = movies.Where(m => m.Ratings.Any() && m.Ratings.Average() >= minRating.Value).ToList();
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
            movie.EntryType = "metadata";

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

            var movie = await _dynamoDbContext.LoadAsync<Movie>(id, "metadata");

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

            var movie = await _dynamoDbContext.LoadAsync<Movie>(id, "metadata");

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
                await _dynamoDbContext.SaveAsync(movie);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving movie: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
