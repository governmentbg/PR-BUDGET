using Microsoft.AspNetCore.Mvc;

namespace CielaDocs.SjcWeb.Areas.ProsecutorAdmin.Controllers
{
    [Area("ProsecutorAdmin")]
    public class HelpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
