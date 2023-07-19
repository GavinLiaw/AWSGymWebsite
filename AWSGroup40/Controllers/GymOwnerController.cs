using Microsoft.AspNetCore.Mvc;

namespace AWSGymWebsite.Controllers
{
    public class GymOwnerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
