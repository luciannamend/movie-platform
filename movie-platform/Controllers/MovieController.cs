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
        public async Task<IActionResult> Index()
        {
            var movies = await _dynamoDbContext.ScanAsync<Movie>(new List<ScanCondition>()).GetRemainingAsync();
            return View(movies);
        }

        // GET: Movie/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var movie = await _dynamoDbContext.LoadAsync<Movie>(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

    }
}
