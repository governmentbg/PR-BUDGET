using AutoMapper;

using CielaDocs.Application;
using CielaDocs.Application.Common.Constants;
using CielaDocs.Application.Dtos;
using CielaDocs.Shared.Repository;
using CielaDocs.AdminPanel.Extensions;
using CielaDocs.AdminPanel.Models;
using CielaDocs.Application.Utils;




using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CielaDocs.Application.Models;
using CielaDocs.Shared.Services;
using CielaDocs.Domain.Entities.v2;
using CielaDocs.Domain.Entities;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace CielaDocs.AdminPanel.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Policy = "AdminOnly")]
    public class HomeController : CommonController
    {

        
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISjcService _sjcService;
        private readonly ISjcServiceV2 _sjcServiceV2;

        public HomeController( ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IHttpContextAccessor httpContextAccessor,ISjcService sjcService,ISjcServiceV2 sjcServiceV2)
        {

            
            _logRepo = logRepo;
            _sjcRepo = sjcRepo;
            _httpContextAccessor = httpContextAccessor;
            _sjcService= sjcService;
            _sjcServiceV2 = sjcServiceV2;
        }

        public async Task<IActionResult> Index()
        {

            //var dbUser = await Mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            var dbUser = await CurrentEmpl.GetCurrentEmplAsync();
            var cfg = await _sjcService.GetCfg();
            ViewBag.CourtId = dbUser?.CourtId ?? 0;
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            string logmsg = $"Администриране и конфигуриране на системата от {User?.Identity?.Name ?? string.Empty}";
            await _logRepo.AddToAppUserLogAsync(new Domain.Entities.AppUserLog { AppUserId = dbUser?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });
            ViewBag.OfficialCurrencyCode=cfg.OfficialCurrencyCode;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InitFinYear(int? id)
        {
            var empl = await CurrentEmpl.GetCurrentEmplAsync();
            if ((!empl.CanAdd) && (!empl.CanUpdate))
            {
                return Json(new { msg = "Нямате предоставени права да добавяте/редактирате данни ", success = false, id = 0 });
            }

            try
            {
                var res = await _sjcRepo.Sp_InitFinYearStage1Async(id ?? 0);
                var res2 = await _sjcRepo.Sp_InitFinYearStage2Async(id ?? 0);
                return Json(new { msg = "Годишната инициализация завърши", success = true });
            }
            catch (Exception ex)
            {
                return Json(new { msg = $"Годишната инициализация завърши с грешка {ex?.Message} ", success = false });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InitProgramData(int? id)
        {
            var empl = await Mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if ((!empl.CanAdd) && (!empl.CanUpdate))
            {
                return Json(new { msg = "Нямате предоставени права да добавяте/редактирате данни ", success = false, id = 0 });
            }

            for (int i = 1; i <= 9; i++)
            {
                try
                {
                    var res = await _sjcRepo.Sp_InitProgramDataAsync(i, id ?? 0);
                    var res2 = await _sjcRepo.Sp_InitProgramDataCourtAsync(i, id ?? 0);
                    var res3 = await _sjcRepo.Sp_InitProgramDataInstitutionAsync(i, id ?? 0);
                }
                catch (Exception ex)
                {
                    return Json(new { msg = $"Годишната инициализация на програма {i} за година {id} завърши с грешка {ex?.Message} ", success = false });
                }
            }
            return Json(new { msg = $"Годишната инициализация на програми за  {id} година завърши", success = true });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgramData(int? id)
        {
            var empl = await Mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if ((!empl.CanAdd) && (!empl.CanUpdate))
            {
                return Json(new { msg = "Нямате предоставени права да добавяте/редактирате данни ", success = false, id = 0 });
            }

            for (int i = 1; i <= 9; i++)
            {
                try
                {
                    var res = await _sjcRepo.Sp_UpdateProgramDataAsync(i, id ?? 0);

                    var res2 = await _sjcRepo.Sp_UpdateProgramDataCourtAsync(i, id ?? 0);

                    var res3 = await _sjcRepo.Sp_UpdateProgramDataInstitutionAsync(i, id ?? 0);
                }
                catch (Exception ex)
                {
                    return Json(new { msg = $"Годишната инициализация на програма {i} за година {id} завърши с грешка {ex?.Message} ", success = false });
                }
            }
            return Json(new { msg = $"Годишната инициализация на програми за  {id} година завърши", success = true });
        }
        [HttpGet]
        public async Task<JsonResult> GetBudgetPeriods()
        {
            try
            {
                var data = await _sjcServiceV2.GetBudgetPeriodsAsync();
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<BudgetPeriodVm>());
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> EndActivePeriodById(int? id) {
            var empl = await Mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if ((!empl.CanAdd) && (!empl.CanUpdate))
            {
                return Json(new { msg = "Нямате предоставени права да добавяте/редактирате данни ", success = false, id = 0 });
            }
            var data = await _sjcServiceV2.GetActiveBudgetPeriodByIdAsync(id??0);
            if (data?.IsActive != true) {
                return Json(new { msg = "Избраният период не е активен!", success = false, id = 0 });
            }
            try
            {
                var pdhinput = await _sjcServiceV2.GetProgramDataForEndingPeriod(id ?? 0);
                bool itemExists = false;
                if (pdhinput.Any())
                {
                    foreach (var item in pdhinput)
                    {
                        itemExists = await _sjcServiceV2.GetProgramDataHExistsAsync(id, item?.FunctionalSubAreaId, item?.RowNum, item?.PlannedYear1);
                        if (!itemExists)
                        {
                            _ = await _sjcServiceV2.InsertIntoProgramDataHAsync(item, id ?? 0);
                        }
                    }
                }
                var pdchInput = await _sjcServiceV2.GetProgramDataCourtForEndingPeriod(id ?? 0);
                if (pdchInput.Any())
                {
                    foreach (var item in pdchInput)
                    {
                        itemExists = await _sjcServiceV2.GetProgramDataCourtHExistsAsync(id, item?.CourtId ?? 0, item?.FunctionalSubAreaId, item?.RowNum, item?.PlannedYear1);
                        if (!itemExists)
                        {
                            _ = await _sjcServiceV2.InsertIntoProgramDataCourtHAsync(item, id ?? 0);
                        }
                    }
                }
                var pdihInput = await _sjcServiceV2.GetProgramDataInstitutionForEndingPeriod(id ?? 0);
                if (pdihInput.Any())
                {
                    foreach (var item in pdihInput)
                    {
                        itemExists = await _sjcServiceV2.GetProgramDataInstitutionHExistsAsync(id, item?.InstitutionTypeId ?? 0, item?.FunctionalSubAreaId, item?.RowNum, item?.PlannedYear1);
                        if (!itemExists)
                        {
                            _ = await _sjcServiceV2.InsertIntoProgramDataInstitutionHAsync(item, id ?? 0);
                        }
                    }
                }
                var idhinput = await _sjcServiceV2.GetIndicatorDataForEndingPeriod(id ?? 0);
                itemExists = false;
                if (idhinput.Any())
                {
                    foreach (var item in idhinput)
                    {
                        itemExists = await _sjcServiceV2.GetIndicatorDataHExistsAsync(id, item?.FunctionalSubAreaId, item?.Id, item?.PlannedYear1);
                        if (!itemExists)
                        {
                            _ = await _sjcServiceV2.InsertIntoIndicatorDataHAsync(item, id ?? 0);
                        }
                    }
                }
                var idchinput = await _sjcServiceV2.GetIndicatorDataCourtForEndingPeriod(id ?? 0);
                itemExists = false;
                if (idchinput.Any())
                {
                    foreach (var item in idchinput)
                    {
                        itemExists = await _sjcServiceV2.GetIndicatorDataCourtHExistsAsync(id,item?.CourtId, item?.FunctionalSubAreaId, item?.Id, item?.PlannedYear1);
                        if (!itemExists)
                        {
                            _ = await _sjcServiceV2.InsertIntoIndicatorDataCourtHAsync(item, id ?? 0);
                        }
                    }
                }
                _ = await _sjcServiceV2.SpDeleteEndPeriodDataAsync(id ?? 0);
                return Json(new { msg = $"Приключването на бюджетния период завърши! Изберете следващ активен бюджетен период", success = true });
            }
            catch (Exception ex) {
                return Json(new { msg = $"Грешка при приключване на бюджетен период {ex?.Message}", success = false });
            }
            
        }

        public PartialViewResult AddBudgetPeriodPartial() => PartialView("AddBudgetPeriodPartial");
        public async Task<PartialViewResult> AddCurrentYear()
        {
            ViewBag.CurrentYear = await _sjcService.QueryRaw<int>($"Select CurrentYear from Cfg");
           return PartialView("AddCurrentYear"); 
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddNewActivePeriod(int ny)
        {
            var empl = await Mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if ((!empl.CanAdd) && (!empl.CanUpdate))
            {
                return Json(new { msg = "Нямате предоставени права да добавяте/редактирате данни ", success = false, id = 0 });
            }
            List<int> years = new List<int> { ny, ny+1, ny+2, ny+3 };
            try
            {
                int? newId = await _sjcService.ExecuteRawSql($@"INSERT INTO BudgetPeriod
                           ([Y1]
                           ,[Y2]
                           ,[Y3]
                           ,[Y4]
                           ,[IsActive]
                           ,[IsUsable]
                           ,[ActiveFrom]
                           )
                     VALUES
                           ({ny}
                           ,{ny + 1}
                           ,{ny + 2}
                           ,{ny + 3}
                           ,{1}
                           ,{1}
                           ,'{CielaDocs.Application.Utils.Utils.GetSqlDateTime(DateTime.Now, 0)}')
                            SELECT SCOPE_IDENTITY() AS LastInsertedId;");
                foreach (int year in years)
                {
                 

                    for (int i = 1; i <= 9; i++)
                    {
                        
                           _ = await _sjcRepo.Sp_InitProgramDataAsync(i, year);
                           _ = await _sjcRepo.Sp_InitProgramDataCourtAsync(i, year);
                           _ = await _sjcRepo.Sp_InitIndicatorDataAsync(i, year, newId??0);
                           _ = await _sjcRepo.Sp_InitIndicatorDataCourtAsync(i, year,newId??0);
                           _ = await _sjcRepo.Sp_InitProgramDataInstitutionAsync(i, year);
                      
                    }
                    for (int i = 1; i <= 9; i++)
                    {
                      
                          _ = await _sjcRepo.Sp_UpdateProgramDataAsync(i, year);
                          _ = await _sjcRepo.Sp_UpdateProgramDataCourtAsync(i, year);
                          _ = await _sjcRepo.Sp_UpdateProgramDataInstitutionAsync(i, year);
                      
                    }
                }
                

                return Json(new { msg = $"Създаването на новия активен бюджетен период завърши", success = true });

            }
            catch (Exception ex) {
                return Json(new { msg = $"Създаването на новия активен бюджетен период завърши с грешка {ex?.Message} ", success = false });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddNewCurrentYear(int ny)
        {
            var empl = await Mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if ((!empl.CanAdd) && (!empl.CanUpdate))
            {
                return Json(new { msg = "Нямате предоставени права да добавяте/редактирате данни ", success = false, id = 0 });
            }

            try
            {
                _ = await _sjcServiceV2.SpEndCurrentYearDataAsync();

                int? newId = await _sjcService.ExecuteRawSql($@"Update Cfg set CurrentYear={ny}");
            

                    for (int i = 1; i <= 9; i++)
                    {

                        _ = await _sjcRepo.Sp_InitProgramDataAsync(i, ny);
                       _ = await _sjcRepo.Sp_InitProgramDataInstitutionAsync(i, ny);

                    }
                    for (int i = 1; i <= 9; i++)
                    {

                        _ = await _sjcRepo.Sp_UpdateProgramDataAsync(i, ny);
                        _ = await _sjcRepo.Sp_UpdateProgramDataInstitutionAsync(i, ny);

                    }
                


                return Json(new { msg = $"Създаването на новата текуща бюджетна година завърши", success = true });

            }
            catch (Exception ex)
            {
                return Json(new { msg = $"Създаването на новата текуща бюджетна година завърши с грешка {ex?.Message} ", success = false });
            }
        }
    }
}
