using Microsoft.AspNetCore.Mvc;

using System.Web;

namespace CielaDocs.SjcWeb.Controllers
{
    public class PdfViewerController : Controller
    {
        public IActionResult Index(string id)
        {
            ViewBag.url=id;
            return View();
        }
    }
}
