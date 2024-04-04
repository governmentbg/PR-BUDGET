using CielaDocs.SjcWeb.ViewModels;

using DevExpress.AspNetCore.Spreadsheet;

using DocumentFormat.OpenXml.Wordprocessing;

using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

using Newtonsoft.Json;

namespace CielaDocs.SjcWeb.Controllers
{

    [IgnoreAntiforgeryToken]
    public class WordViewerController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public WordViewerController(IWebHostEnvironment env)
        {
            _env = env;
        }
        public static Stream GetDocumentContentStream(string file)
        {
            return new MemoryStream(System.IO.File.ReadAllBytes(file));
        }

        public async Task<IActionResult> Index(string id, string filePath)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                string wordFile = System.IO.Path.Combine(_env.WebRootPath + "/Temp/", id);
                var fileExtension = System.IO.Path.GetExtension(id).ToLower();
                if (wordFile == null || (!fileExtension.EndsWith("docx") && !fileExtension.EndsWith("rtf") && !fileExtension.EndsWith("txt")))
                    throw new ArgumentException($"Cannot edit files with the extension '{fileExtension}'.");
                var model = new DocumentViewModel()
                {
                    FileName = id,
                    DocumentBytes = await System.IO.File.ReadAllBytesAsync(wordFile, default)

                };
                return View("Index", model);
            }
            else if (!string.IsNullOrWhiteSpace(filePath)) {

               
                var fileExtension = System.IO.Path.GetExtension(filePath).ToLower();
                if (filePath == null || (!fileExtension.EndsWith("docx") && !fileExtension.EndsWith("rtf") && !fileExtension.EndsWith("txt")))
                    throw new ArgumentException($"Cannot edit files with the extension '{fileExtension}'.");
                var model = new DocumentViewModel()
                {
                    FileName = id,
                    DocumentBytes = await System.IO.File.ReadAllBytesAsync(filePath, default)

                };
                return View("Index", model);

            }
            else
            {
                return View("Index", new DocumentViewModel { });
            }
  
        }
        #region LoadAndSave
        public IActionResult LoadAndSave()
        {
            return View();
        }

        public IActionResult Export(string base64, string fileName, DevExpress.AspNetCore.RichEdit.DocumentFormat format)
        {
            byte[] fileContents = System.Convert.FromBase64String(base64);
            return Ok();
        }

        public IActionResult DocumentProtection()
        {
            return View();
        }
        #endregion


       

    }
}
