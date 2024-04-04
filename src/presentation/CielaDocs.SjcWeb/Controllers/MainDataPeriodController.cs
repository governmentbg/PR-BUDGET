using CielaDocs.Application;
using CielaDocs.Application.Models;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.ExpressionEngine;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;
using CielaDocs.SjcWeb.Extensions;
using CielaDocs.SjcWeb.Models;

using DocumentFormat.OpenXml.Bibliography;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace CielaDocs.SjcWeb.Controllers
{
    public class MainDataPeriodController : Controller
    {
        private readonly ILogger<MainDataController> _logger;
        private readonly IMediator _mediator;
        private readonly ISendGridMailer _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IWebHostEnvironment _env;

        private FilterMainDataVm? FilterData = null;

        public MainDataPeriodController(ILogger<MainDataController> logger, IConfiguration configuration, ISendGridMailer emailSender,
                        IMediator mediator, IHttpContextAccessor httpContextAccessor, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IWebHostEnvironment env)
        {
            _logger = logger;
            _mediator = mediator;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _logRepo = logRepo;
            _sjcRepo = sjcRepo;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            FilterData = HttpContext.Session.Get<FilterMainDataVm>("FilterMainDataSess") ?? new FilterMainDataVm();
            var court = await _mediator.Send(new GetCourtByIdQuery { Id = FilterData?.CourtId ?? 0 });
            var fsub = await _mediator.Send(new GetFunctionalSubAreaByIdQuery { Id = FilterData?.FunctionalSubAreaId ?? 0 });
            var farea = await _mediator.Send(new GetFunctionalAreabyIdQuery { Id = fsub?.FunctionalAreaId ?? 0 });
            ViewData["court"] = court;
            ViewBag.Month = FilterData?.Nmonth;
            ViewBag.Year = FilterData?.Nyear;
            ViewBag.FunctionalSubAreaName = fsub?.Name;
            ViewBag.FunctionalAreaName = farea?.Name;
            return View();
        }
        [HttpGet]

        public async Task<JsonResult> GetMainDataGrid(int? typeOfIndicatorId)
        {
            try
            {
                FilterData = HttpContext.Session.Get<FilterMainDataVm>("FilterMainDataSess") ?? new FilterMainDataVm();
                var data = await _sjcRepo.GetMainPeriodGridByFilterAsync(FilterData?.FunctionalSubAreaId ?? 0, FilterData?.CourtId ?? 0, FilterData?.Nmonth ?? 0, FilterData?.Nyear ?? 0);
                return (typeOfIndicatorId != null) ? Json(data.ToList().Where(x => x.TypeOfIndicatorId == typeOfIndicatorId)) : Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<MainDataGrid>());
            }
        }

        [HttpGet]
        public async Task<PartialViewResult> GetMainDataItemsPartial(int? id)
        {
            if ((id == null) || (id < 0))
            {
                return PartialView("_ErrorPartialView", "Невалиден указател!");
            }
            FilterData = HttpContext.Session.Get<FilterMainDataVm>("FilterMainDataSess") ?? new FilterMainDataVm();
            var md = await _sjcRepo.GetMainDataPeriodByIdAsync(id ?? 0);
            var mi = await _sjcRepo.GetMainIndicatorsByIdAsync(md?.MainIndicatorsId ?? 0);
            List<string> vars = new();
            var t = new Tokenizer(new StringReader(mi?.Calculation ?? string.Empty));
            while (t.Token != Token.EOF)
            {
                if (vars.IndexOf(t.Identifier) < 0)
                { vars.Add(t.Identifier); }

                t.NextToken();
            }
            var metricsFields = await _sjcRepo.GetMetricsFiledByCode(FilterData?.CourtId ?? 0, FilterData?.Nmonth ?? 0, FilterData?.Nyear ?? 0, string.Join(',', vars.ToArray()));
            switch (vars.Count)
            {
                case 0:
                    return PartialView("MainDataItem0PartialView");

                case 1:
                case 2:
                case 3:
                case 4:
                    ViewBag.MainDataId = id ?? 0;
                    ViewBag.MainIndicatorsId = md?.MainIndicatorsId ?? 0;
                    return PartialView("MainDataItemsPartialView", metricsFields);


                default:
                    return PartialView("MainDataItem0PartialView");
            }

        }
        private string ReplaceCalculationFormula(string Source, Dictionary<string, string> dic)
        {try
            {
                // string result=string.Empty;
                foreach (var (key, value) in dic)
                {
                    int Place = Source.IndexOf(key);

                    Source = Source.Remove(Place, key.Length).Insert(Place, value);


                }

                return Source;
            }
            catch { return "0/0"; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveMainDataItems(IEnumerable<MainDataItemsResult> record)
        {

            if (ModelState.IsValid)
            {

                try
                {
                    int.TryParse(Request.Form["MainDataId"].ToString(), out int nMainDataId);
                    int.TryParse(Request.Form["MainIndicatorsId"].ToString(), out int nMainIndicatorsId);
                    _ = await _sjcRepo.UpdateMainDataItemByIdAsync(record);

                    //---------------------calc------------------
                    var mi = await _sjcRepo.GetMainIndicatorsByIdAsync(nMainIndicatorsId);


                    var dic = new Dictionary<string, string>();
                    foreach (var itm in record)
                    {
                        if (!dic.ContainsKey(itm.Code))
                        {
                            dic.Add(itm.Code, itm.Nvalue.ToString());
                        }
                    }
                    string calculationString = ReplaceCalculationFormula(mi.Calculation ?? string.Empty, dic);

                    var res = Parser.Parse(calculationString).Eval(null);
                    if (mi?.MeasureId == 1)
                    {
                        res = res * 100;
                    }
                    var ok = await _sjcRepo.UpdateMainPeriodValueByIdAsync(nMainDataId, res);

                    return Json(new { msg = "Данните бяха редактирани", success = true });
                }
                catch (Exception ex)
                {
                    string messages = string.Join("; ", ModelState.Values
               .SelectMany(x => x.Errors)
               .Select(x => x.ErrorMessage));
                    return Json(new { msg = messages, success = false });
                }


            }
            else
            {
                string messages = string.Join("; ", ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage));
                return Json(new { msg = messages, success = false });
            }
        }
        private bool ValidateString(string s) {
            char[] enabledcharc = new char[] { '*', '/', '+', '-', '(', ')', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',' ','.' };
            foreach (var c in s)
            {
                if (!enabledcharc.Contains(c))
                {
                    return false;
                }
            }
            return true;
        }
        public async Task<IActionResult> RecalculateMainData()
        {
            try
            {
               
                FilterData = HttpContext.Session.Get<FilterMainDataVm>("FilterMainDataSess") ?? new FilterMainDataVm();
                var data = await _sjcRepo.GetMainPeriodGridByFilterAsync(FilterData?.FunctionalSubAreaId ?? 0, FilterData?.CourtId ?? 0, FilterData?.Nmonth ?? 0, FilterData?.Nyear ?? 0);
                foreach (var row in data)
                {
                    var mi = await _sjcRepo.GetMainIndicatorsByIdAsync(row?.MainIndicatorsId ?? 0);
                    var parameters = Toolbox.ExtractCalcArgs(mi?.Calculation);
            
                    if (parameters.Any())
                    {
                        

                        var dic = new Dictionary<string, string>();
                        foreach (var item in parameters)
                        {
                            if (item == "TMonths")
                            {
                                dic.Add(item, "1");
                            }
                            if (item == "NJManMonth")
                            {
                                dic.Add(item, "1");
                            }
                            if (item == "NMagTo")
                            {
                                dic.Add(item, "1");
                            }
                            if (item == "NOne")
                            {
                                dic.Add(item, "1");
                            }
                            if (item == "NTimeCaseCompl")
                            {
                                dic.Add(item, "1");
                            }
                        }
                        var maindataitems = await _sjcRepo.GetMainPeriodItemFiledByCodes(FilterData?.CourtId ?? 0, FilterData?.Nmonth ?? 0, FilterData?.Nyear ?? 0, string.Join(",", parameters));
                        maindataitems = maindataitems.Where(x => x.Nvalue != null);
                        if (maindataitems.Any())
                        {
                            foreach (var itm in maindataitems)
                            {
                                if (!dic.ContainsKey(itm.Code))
                                {
                                    dic.Add(itm.Code, itm?.Nvalue?.ToString());
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(mi?.Calculation))
                            {
                                string calculationString = ReplaceCalculationFormula(mi?.Calculation ?? string.Empty, dic);
                                if (ValidateString(calculationString))
                                {
                                    try
                                    {
                                        var res = Parser.Parse(calculationString).Eval(null);
                                        bool isNaN = Double.IsNaN(res);
                                        if (isNaN) res = 0;
                                        if (mi?.MeasureId == 1)
                                        {
                                            res = res * 100;
                                        }
                                        var ok = await _sjcRepo.UpdateMainPeriodValueByIdAsync(row?.Id, res);
                                    }
                                    catch (Exception ex)
                                    {
                                        return Json(new { msg = "Error calculation: " + ex?.Message, success = false });
                                    }
                                }
                            }
                        }





                    }
                }

                return Json(new { msg = "Показателите бяха преизчислени", success = true });
            }
            catch (Exception ex)
            {
                return Json(new { msg = $"Показателите не бяха преизчислени. Грешка:{ex?.Message} ", success = false });
            }
        }
        [HttpPost]

        public async Task<JsonResult> UpdateMainData(int key, string values)
        {
            var nv = new EnteredValues();
            JsonConvert.PopulateObject(values, nv);
            _ = await _sjcRepo.UpdateMainPeriodEnteredValueByIdAsync(key, nv.EnteredValue);
            return Json(string.Empty);
        }
    }
}
