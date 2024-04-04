using Microsoft.AspNetCore.Mvc;

namespace CielaDocs.SjcWeb.Controllers
{
    public class HelpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
