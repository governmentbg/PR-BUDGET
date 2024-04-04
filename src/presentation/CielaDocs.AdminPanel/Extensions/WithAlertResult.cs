using CielaDocs.AdminPanel.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

namespace CielaDocs.AdminPanel.Extensions
{
    public class WithAlertResult : IActionResult
    {
        public IActionResult Result { get; }

        public Alert Alert { get; }

        public WithAlertResult(IActionResult result,
                               string type,
                               string message,
                               string debugInfo,
                               string actionText,
                               string actionUrl)
        {
            Result = result;
            Alert = new Alert
            {
                Type = type,
                Message = message,
                DebugInfo = debugInfo,
                ActionText = actionText,
                ActionUrl = actionUrl,
            };
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var factory = context.HttpContext.RequestServices
            .GetService<ITempDataDictionaryFactory>();

            var tempData = factory.GetTempData(context.HttpContext);

            tempData.AddAlert("_alertData", Alert);

            await Result.ExecuteResultAsync(context);
        }
    }
}
