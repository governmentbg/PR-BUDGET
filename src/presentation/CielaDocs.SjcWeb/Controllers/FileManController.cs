using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.IO.Compression;
using CielaDocs.SjcWeb.Models;
using MediatR;
using CielaDocs.Application;
using CielaDocs.SjcWeb.Extensions;

namespace CielaDocs.SjcWeb.Controllers
{
    [Route("api/[controller]")]
    public class FileManController : Controller
    {
        private string rootFolderName;
        private readonly IMediator _mediator;

        public FileManController(IWebHostEnvironment env, IMediator mediator)
        {
            _env = env;
            rootFolderName = System.IO.Path.Combine(_env.WebRootPath + "/uploads/");
            _mediator=mediator;
        }
      
        private readonly IWebHostEnvironment _env;
     
        [Route("GetItems")]
        [HttpGet]
        public string GetItems(string pathInfo)
        {
            var folderName = pathInfo == null ? rootFolderName : pathInfo;


            DirectoryInfo dirInfo = new DirectoryInfo(folderName);
            List<FileDataItem> items = new List<FileDataItem>();
            foreach (DirectoryInfo dinfo in dirInfo.GetDirectories())
            {
                items.Add(new FileDataItem() { Key = dinfo.FullName, IsDirectory = true, Created = dinfo.CreationTime, Name = dinfo.Name, HasSubDirectories = dinfo.GetDirectories().Length > 0 });
            }
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                items.Add(new FileDataItem() { Key = file.FullName, IsDirectory = false, Created = file.CreationTime, Name = file.Name, HasSubDirectories = false });
            }
            return this.CreateResponse(true, null, null, items.ToArray());
        }

        [Route("CreateDirectory")]
        [HttpGet]
        public string CreateDirectory(string pathInfo, string name)
        {
            string response = string.Empty;
            if (pathInfo == null)
            {
                response = this.CreateResponse(false, 409, "You can't create a folder in the root directory", null);
            }
            else
            {
                var folderName = pathInfo;
                DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(pathInfo, name));
                if (dir != null)
                {
                    response = this.CreateResponse(true, null, null, null);
                }
            }
            return response;
        }

        [Route("DownloadItems/{name}")]
        public FileResult DownloadItems(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            var items = JsonConvert.DeserializeObject<string[]>(name);
            if (items.Length == 1)
            {
                var filePath = items[0];
                return File(new FileStream(filePath, FileMode.Open), "application/octet-stream");
            }
            else
            {
                byte[] compressedBytes;
                using (var resultStream = new MemoryStream())
                {
                    //zip
                    using (var archive = new ZipArchive(resultStream, ZipArchiveMode.Create, true))
                    {
                        for (int i = 0; i < items.Length; i++)
                        {
                            var filePath = items[i];
                            var fileInArchive = archive.CreateEntry(filePath, CompressionLevel.Optimal);
                            var fileBytes = System.IO.File.ReadAllBytes(filePath);
                            using (var entryStream = fileInArchive.Open())
                            using (var fileToCompressStream = new MemoryStream(fileBytes))
                            {
                                fileToCompressStream.CopyTo(entryStream);
                            }
                        }
                    }
                    compressedBytes = resultStream.ToArray();
                }
                return File(compressedBytes, "application/octet-stream");
            }

        }

        [Route("DeleteItem")]
        [HttpGet]
        public async Task<string> DeleteItem(string pathInfo)
        {
            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl?.CanDelete == false) {
                return  this.CreateResponse(false, 409, "Нямате предоставени права за това действие", null);
            }
            string response = string.Empty;
            if (Directory.Exists(pathInfo))
            {
                DirectoryInfo di = new DirectoryInfo(pathInfo);
                if (di.GetFiles().Length > 0 || di.GetDirectories().Length > 0)
                {
                    response = this.CreateResponse(false, 409, "You can't remove a directory with files or other directories", null);
                }
                else
                {
                    Directory.Delete(pathInfo);
                    response = this.CreateResponse(true, null, null, null);
                }
            }
            else
            {
                FileInfo di = new FileInfo(pathInfo);
                di.Delete();
                response = this.CreateResponse(true, null, null, null);
            }
            return response;
        }

        [Route("RenameItem")]
        [HttpGet]
        public string RenameItem(string pathInfo, string newName)
        {
            string response = string.Empty;
            if (Directory.Exists(pathInfo))
            {
                DirectoryInfo di = new DirectoryInfo(pathInfo);
                string folderName = Path.GetDirectoryName(pathInfo);
                string newDirectory = Path.Combine(di.Parent.FullName, newName);
                if (Directory.Exists(newDirectory))
                {
                    return this.CreateResponse(false, 409, "The target directory already exists", null);
                }
                di.MoveTo(newDirectory);
            }
            else
            {
                FileInfo fi = new FileInfo(pathInfo);
                string newpath = Path.Combine(fi.Directory.FullName, newName);
                FileInfo nfi = new FileInfo(newpath);
                if (nfi.Exists)
                {
                    return this.CreateResponse(false, 409, "The target file already exists", null);
                }
                fi.MoveTo(newpath);
            }
            return this.CreateResponse(true, null, null, null); ;
        }
        string CreateResponse(bool success, int? errorCode, string errorText, FileDataItem[] result)
        {
            var serializerSettings = new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            return JsonConvert.SerializeObject(new { success, errorCode, errorText, result }, serializerSettings);
        }

        void CopyDirectory(string sourceDir, string destinationDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            destinationDir = Path.Combine(destinationDir, dir.Name);
            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destinationDir);
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }

        [Route("MoveItem")]
        [HttpGet]
        public string MoveItem(string pathInfo, string destinationDirectory)
        {
            if (string.IsNullOrEmpty(destinationDirectory))
            {
                destinationDirectory = rootFolderName;
            }
            if (Directory.Exists(pathInfo))
            {
                var sourceDirectory = new DirectoryInfo(pathInfo);
                var targetDirectory = new DirectoryInfo(destinationDirectory);
                if (targetDirectory.GetDirectories().FirstOrDefault(d => d.Name == sourceDirectory.Name) != null)
                {
                    return this.CreateResponse(false, 409, "The dirctory already exists in the destination folder", null);
                }
                else
                {
                    CopyDirectory(pathInfo, destinationDirectory);
                    Directory.Delete(pathInfo, true);
                }
            }
            else
            {
                FileInfo fi = new FileInfo(pathInfo);
                string newpath = Path.Combine(destinationDirectory, fi.Name);
                FileInfo nfi = new FileInfo(newpath);
                if (nfi.Exists)
                {
                    return this.CreateResponse(false, 409, "The file already exists in the destination folder", null);
                }
                fi.MoveTo(newpath);
            }
            return this.CreateResponse(true, null, null, null); ;
        }

        [Route("CopyItem")]
        [HttpGet]
        public string CopyItem(string pathInfo, string destinationDirectory)
        {
            if (string.IsNullOrEmpty(destinationDirectory))
            {
                destinationDirectory = rootFolderName;
            }
            if (Directory.Exists(pathInfo))
            {
                var sourceDirectory = new DirectoryInfo(pathInfo);
                var targetDirectory = new DirectoryInfo(destinationDirectory);
                if (targetDirectory.GetDirectories().FirstOrDefault(d => d.Name == sourceDirectory.Name) != null)
                {
                    return this.CreateResponse(false, 409, "The dirctory already exists in the destination folder", null);
                }
                else
                {
                    CopyDirectory(pathInfo, destinationDirectory);
                }
            }
            else
            {
                FileInfo fi = new FileInfo(pathInfo);
                string newpath = Path.Combine(destinationDirectory, fi.Name);
                FileInfo nfi = new FileInfo(newpath);
                if (nfi.Exists)
                {
                    return this.CreateResponse(false, 409, "The file already exists in the destination folder", null);
                }
                fi.CopyTo(newpath);
            }
            return this.CreateResponse(true, null, null, null); ;
        }

        [Route("UploadFileChunk")]
        [HttpPost]
        public string UploadChunk(IFormFile fileChunk)
        {
            if (Request.HasFormContentType)
            {
                Request.Form.TryGetValue("chunkMetadata", out var metadataSerialized);
                Request.Form.TryGetValue("destinationDirectory", out var destinationDirSerialized);
                var chunkMetadata = JsonConvert.DeserializeObject<ChunkMetadata>(metadataSerialized);
                var destinationDirectory = JsonConvert.DeserializeObject<FileDataItem>(destinationDirSerialized);
                SaveFile(fileChunk, chunkMetadata.fileName, Path.Combine(rootFolderName, destinationDirectory.Key));
                return this.CreateResponse(true, null, null, null); ;
            }
            else
            {
                return this.CreateResponse(false, 409, "An error occured!", null);
            }
        }

        [NonAction]
        void SaveFile(IFormFile file, string fileName, string destinationFolder)
        {
            var path = Path.Combine(destinationFolder, fileName);
            using (var tempFile = System.IO.File.Open(path, FileMode.Append))
            {
                file.CopyTo(tempFile);
            }
        }
    }
}
