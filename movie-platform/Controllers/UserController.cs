using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using movie_platform.Models;

namespace movie_platform.Controllers
{
    public class UserController : Controller
    {
        private readonly MovieplatformdbContext _context;

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
            // create id
            user.Id = new Random().Next(1, 1000000);
            // hash password
            var hashedPassword = HashPassword(user.Password);
            user.Password = hashedPassword;

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

            var hashedPassword = HashPassword(user.Password);
            user.Password = hashedPassword;

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
        public async Task<IActionResult> Login([Bind("UserName,Password")] Login login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }

            // Check if user exists with matching username 
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == login.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(login);
            }
            //Debug.WriteLine($"Route for {nameof(Index)}: {Request.Path}");

            // Verify the password
            if (!VerifyPassword(login.Password, user.Password))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(login);
            }

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

        // SERVICE
        // check if user exists
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        // encrypt the password
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // verify the hashed password
        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
