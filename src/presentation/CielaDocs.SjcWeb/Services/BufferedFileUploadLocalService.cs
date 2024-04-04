using CielaDocs.Shared.Services;

namespace CielaDocs.SjcWeb.Services
{
    public class BufferedFileUploadLocalService : IBufferedFileUploadService
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public BufferedFileUploadLocalService(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<string> UploadFile(IFormFile file)
        {
            string path = "";
            try
            {
                if (file.Length > 0)
                {
                    path = Path.GetFullPath(Path.Combine(_hostingEnvironment.WebRootPath, "Temp"));
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    using (var fileStream = new FileStream(Path.Combine(path, file.FileName), FileMode.Create))
                    {
                        try
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        catch (Exception ex)
                        {
                            var message = ex.ToString();
                        }
                    }
                    return file.FileName;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("File Copy Failed", ex);
            }
        }
    }
}
