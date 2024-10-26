using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using movie_platform.Models;

namespace movie_platform.Controllers
{
    public class UserController : Controller
    {
        private readonly MovieplatformdbContext _context;
        private static int _nextId = 1;

        public UserController(MovieplatformdbContext context)
        {
            _context = context;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: User/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserName,Password")] User user)
        {
            user.Id = _nextId++;

            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();

                // if user is not logged in, redirect to login page
                if (HttpContext.Session.GetString("UserName") == null)
                {
                    return RedirectToAction("Login", "User");
                }
                return RedirectToAction(nameof(Index));
            }

            

            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserName,Password")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Login
        [HttpGet]
        [Route("/Login")]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Login
        [HttpPost, ActionName("Login")]
        [Route("/Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("UserName,Password")] LoginView login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }

            // Check if user exists with matching username and password
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == login.UserName && u.Password == login.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(login);
            }
            Debug.WriteLine($"Route for {nameof(Index)}: {Request.Path}");

            // Set a session variable to indicate user is logged in
            HttpContext.Session.SetString("UserName", user.UserName);

            // Redirect to user pg if login is successful 
            return RedirectToAction("Index", "Home");
        }

        // GET: /Logout
        [Route("/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserName"); // Remove the session variable
            return RedirectToAction("Index", "Home"); // Redirect to home or login page
        }


        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
