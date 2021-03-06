﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using login_reg.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace login_reg.Controllers
{
    public class HomeController : Controller
    {
        private UserContext db;
        public HomeController(UserContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            // If there is a user already logged in (=it's in session), jump them in
            int? UserId = HttpContext.Session.GetInt32("UserId");
            if (UserId != null)
            {
                return RedirectToAction("Success");
            }

            return View();
        }

        public IActionResult Register(User newUser)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "is already taken!");
                }
            }
            // Reload with error messages
            if (ModelState.IsValid == false)
            {
                return View("Index");
            }

            // Overwrite password with hashed version
            PasswordHasher<User> hasher = new PasswordHasher<User>();
            newUser.Password = hasher.HashPassword(newUser, newUser.Password);

            // Add attributes
            newUser.CreatedAt = DateTime.Now;
            newUser.UpdatedAt = DateTime.Now;
            
            // Add and save
            db.Users.Add(newUser);
            db.SaveChanges();

            HttpContext.Session.SetInt32("UserId", newUser.UserId);
            return RedirectToAction("Success");
        }

        public IActionResult Login(LoginUser loginUser)
        {
            // Show error messages
            if (ModelState.IsValid == false)
            {
                return View("Index");
            }

            // Get user with matching email from DB
            User dbUser = db.Users.FirstOrDefault(user => user.Email == loginUser.LoginEmail);
            // If no matching users
            if (dbUser == null)
            {
                ModelState.AddModelError("LoginEmail", "Invalid Email or Password");
            }
            else
            {
                PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();
                PasswordVerificationResult result = hasher.VerifyHashedPassword(loginUser, dbUser.Password, loginUser.LoginPassword);
                // If passwords don't match
                if (result == 0)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Email or Password");
                }
            }
            // If either no matching email OR email found but no matching PW, show error messages
            if (ModelState.IsValid == false)
            {
                return View("Index");
            }

            HttpContext.Session.SetInt32("UserId", dbUser.UserId);
            return RedirectToAction("Success");
        }

        [HttpGet("/success")]
        public IActionResult Success()
        {
            // If no user signed in, kick them out
            int? UserId = HttpContext.Session.GetInt32("UserId");
            if (UserId == null)
            {
                return RedirectToAction("Index");
            }

            // User to use in page
            User currentUser = db.Users.FirstOrDefault(user => user.UserId == UserId);
            return View(currentUser);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
