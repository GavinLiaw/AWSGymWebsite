using Microsoft.AspNetCore.Mvc;

namespace AWSGymWebsite.Controllers
{
    public class ViewerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
