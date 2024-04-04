using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CielaDocs.SjcWeb.Controllers
{
    [AllowAnonymous]
    public class VideoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Admin()
        {
            return View();
        }
        public IActionResult Delo()
        {
            return View();
        }
        public IActionResult Messages()
        {
            return View();
        }
        public IActionResult Reports()
        {
            return View();
        }
        public IActionResult Scann()
        {
            return View();
        }
    }
}
