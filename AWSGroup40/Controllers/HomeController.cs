using AWSGroup40.Models;
using AWSGymWebsite.Areas.Identity.Data;
using AWSGymWebsite.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace AWSGroup40.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AWSGymWebsiteUser> _userManager;
        private readonly SignInManager<AWSGymWebsiteUser> _signInManager;

        public HomeController(ILogger<HomeController> logger   
            ,UserManager<AWSGymWebsiteUser> userManager
            ,SignInManager<AWSGymWebsiteUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
           // if (_signInManager.IsSignedIn(User)) {
          //      if (User.IsInRole("GymOwner"))
         //       {
           //         return RedirectToAction("Index", "GymOwner");
        //        }
        //        else {
       //             return RedirectToAction("Index", "Viewer");
       //         }
       //     }
       //     else { 
                return View();
       //     }
            
        }
        public IActionResult goregister()
        {
            return View("Register");
        }
        public IActionResult AboutUs()
        {
            return View();
        }
        public IActionResult ContactUs()
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