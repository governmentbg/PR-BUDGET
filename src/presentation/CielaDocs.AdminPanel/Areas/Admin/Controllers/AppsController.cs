using AutoMapper;

using CielaDocs.Application;
using CielaDocs.Application.Common.Constants;
using CielaDocs.Application.Dtos;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace CielaDocs.AdminPanel.Areas.Admin.Controllers
{
    [Area("admin")]
    public class AppsController : Controller
    {
        private readonly ISjcService _sjcService;

        public AppsController(ISjcService sjcService)
        {
            _sjcService= sjcService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddInstitutionTypePartial(int? appId)
        {
            ViewBag.AppId = appId??0;
           return PartialView(nameof(AddInstitutionTypePartial));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Save(int? appId, int? institutionTypeId)
        {
            try { 
                var rec=await _sjcService.QueryRaw<int?>($"SELECT id FROM AppRequired WHERE appId={appId??0} and InstitutionTypeId={institutionTypeId??0}");
                if ((rec is null) || (rec == 0))
                {
                    _ = await _sjcService.ExecuteRawSql($@"INSERT INTO AppRequired([AppId],[InstitutionTypeId],[IsActive])
                        VALUES({appId},{institutionTypeId},{1}) ");
                }
                    return Json(new { msg = "Данните бяха добавени", success = true });
                
            }
            catch (Exception ex)
            {
                return Json(new { msg = ex.Message, success = false });
            }

        }
        [HttpPost]
        public async Task<ActionResult> DeleteAppRequired(int? id)
        {

            try
            {
                
                    _ = await _sjcService.ExecuteRawSql($@"Delete from AppRequired where id={id??0}");
               
                return Json(new { msg = "Записът бе премахнат", success = true });

            }
            catch (Exception ex)
            {
                return Json(new { msg = ex.Message, success = false });
            }
        }
    }
}
