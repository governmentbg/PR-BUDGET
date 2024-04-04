using CielaDocs.Shared.Services;

using Microsoft.AspNetCore.Mvc;

namespace CielaDocs.SjcWeb.Controllers
{
    public class BufferedFileUploadController : Controller
    {
        readonly IBufferedFileUploadService _bufferedFileUploadService;
        public BufferedFileUploadController(IBufferedFileUploadService bufferedFileUploadService)
        {
            _bufferedFileUploadService = bufferedFileUploadService;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UploadFile()
        {
            if (Request.Form.Files.Any())
            {
                try
                {

                    foreach (var formFile in Request.Form.Files)
                    {
                        if (formFile.Length > 1048576)
                        {
                            return Json(new { success = false, msg = $"Файлът превишава размера от {1048576} байта!" });
                        }
                        var ret = await _bufferedFileUploadService.UploadFile(formFile);
                        if (!string.IsNullOrWhiteSpace(ret))
                        {
                            return Json(new { success = true, msg = "Файлът бе прикачен успешно!", filename = ret });
                        }

                        else return Json(new { success = false, msg = "Файлът не бе прикачен успешно! Опитайте отново.",filename=string.Empty });
                    }
                }

                catch (Exception ex)
                {
                    return Json(new { success = false, msg = $"Грешка:{ex.StackTrace.ToString()}" });

                }

            }

            return Json(new { success = false, msg = "Опитайте отново" });
        }
    }
}
