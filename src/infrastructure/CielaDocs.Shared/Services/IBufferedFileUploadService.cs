using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Shared.Services
{
    public interface IBufferedFileUploadService
    {
        Task<string> UploadFile(IFormFile file);
    }
}
