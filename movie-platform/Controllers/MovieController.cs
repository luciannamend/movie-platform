using System;
using System.Collections.Generic;
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

        // GET: Movie/AddMovie
        public IActionResult AddMovie()
        {
            return View();
        }

        // POST: Movie/AddMovie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMovie(Movie movie)
        {
            if (ModelState.IsValid)
            {
                movie.MovieId = new Random().Next(1, 100000);
                movie.EntryType = "metadata";

                await _dynamoDbContext.SaveAsync(movie);
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        

        //// GET: Movie/Details/5
        //public async Task<IActionResult> Details(int id)
        //{
        //    var aQuitePlace = await _dynamoDbContext.LoadAsync<Movie>(id, "metadata");
        //    if (aQuitePlace == null)
        //    {
        //        return NotFound();
        //    }

        //    // Calculate the average rating for the aQuitePlace
        //    var averageRating = aQuitePlace.Ratings.Any() ? aQuitePlace.Ratings.Average(r => r.Rate) : 0;
        //    ViewData["AverageRating"] = averageRating.ToString("F1");

        //    return View(aQuitePlace);
        //}

        //// GET: Movie/Details/5
        //public async Task<IActionResult> Details(string id)
        //{
        //    var aQuitePlace = await _dynamoDbContext.LoadAsync<Movie>(id);
        //    if (aQuitePlace == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(aQuitePlace);
        //}

    }
}
