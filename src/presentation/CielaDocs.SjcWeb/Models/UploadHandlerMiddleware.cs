using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace CielaDocs.SjcWeb.Models
{
    public class UploadHandlerMiddleware
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UploadHandlerMiddleware(IWebHostEnvironment webHostEnvironment, RequestDelegate next)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task Invoke(HttpContext context)
        {

            try
            {
                if (context.Request.Method == "POST")
                {

                    string inputBody;
                    using (var reader = new System.IO.StreamReader(
                        context.Request.Body, System.Text.Encoding.UTF8))
                    {
                        inputBody = await reader.ReadToEndAsync();
                    }
                    string sAction = HttpUtility.ParseQueryString(inputBody).Get("action");
                    string sName = HttpUtility.ParseQueryString(inputBody).Get("name");
                    string sExt = HttpUtility.ParseQueryString(inputBody).Get("ext");
                    string sData = HttpUtility.ParseQueryString(inputBody).Get("data");

                    switch (sAction.ToLower())
                    {
                        case "create":
                            await context.Response.WriteAsync(this._Create(sExt));
                            break;
                        case "append":
                            this._Append(sName, Convert.FromBase64String(sData));
                            break;
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 400;
                var _msg = string.Empty;
                for (var _ex = ex; _ex != null; _ex = _ex.InnerException)
                {
                    _msg += string.Format("{1}: {2}{0}{3}{0}{0}", Environment.NewLine, ex.GetType().Name, ex.Message, ex.StackTrace);
                }
                await context.Response.WriteAsync(_msg);
            }
        }

        private string _Create(string ext)
        {
            return Path.GetFileName(Path.ChangeExtension(Path.GetTempFileName(), ext));
        }

        private void _Append(string name, byte[] data)
        {
            using (var _stream = File.Open(Path.Combine(_webHostEnvironment.WebRootPath + "/Temp", Path.GetFileName(name)), FileMode.Append))
            {
                _stream.Write(data, 0, data.Length);
            }
        }

    }

    public static class UploadHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseUploadHandlerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UploadHandlerMiddleware>();
        }
    }
}

