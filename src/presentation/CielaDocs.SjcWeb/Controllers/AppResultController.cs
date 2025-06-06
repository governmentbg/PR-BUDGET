using CielaDocs.Application.Models;
using CielaDocs.Shared.ExpressionEngine;
using CielaDocs.Shared.Repository;
using CielaDocs.Shared.Services;
using CielaDocs.SjcWeb.Extensions;
using CielaDocs.SjcWeb.Models;

using ClosedXML.Excel;

using DevExpress.XtraRichEdit.Layout;

using DevExtreme.AspNet.Mvc;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using System.Text;
using System.Text.RegularExpressions;

namespace CielaDocs.SjcWeb.Controllers
{
    public class AppResultController : Controller
    {
        private readonly ILogger<AppResultController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly ISjcService _sjcService;
        private readonly ISjcServiceV2 _sjcServiceV2;
        private const string templateName = "AppResultTemplate.xlsx";
        private const string templateCommonName = "AppResultCommonTemplate.xlsx";
        public AppResultController(ILogger<AppResultController> logger, IWebHostEnvironment env, ISjcService sjcService, ISjcServiceV2 sjcServiceV2)
        {
            _logger = logger;
            _env = env;
            _sjcService = sjcService;
            _sjcServiceV2 = sjcServiceV2;

        }
        private async Task<(string AppName, string ProgramName)> GetProgramNameByAppId(int? appId)
        {
            var app = await _sjcService.QueryRaw<AppVm>("SELECT * FROM App WHERE Id=@Id", new { Id = appId });

            if (app is null || app?.Id == 0)
            {
                return (AppName: "", ProgramName: "");
            }
            int functionalSubAreaId = app.Id switch
            {
                int n when (n >= 1 && n <= 2) => 1,
                int n when (n >= 3 && n <= 4) => 2,
                int n when (n >= 5 && n <= 6) => 3,
                int n when (n >= 7 && n <= 8) => 4,
                int n when (n >= 9 && n <= 10) => 5,
                int n when (n >= 11 && n <= 12) => 6,
                int n when (n >= 13 && n <= 14) => 7,
                int n when (n >= 15 && n <= 16) => 8,
                _ => 0
            };
            string programName = await _sjcService.QueryRaw<string>("SELECT Name FROM FunctionalSubArea WHERE Id=@Id", new { Id = functionalSubAreaId });
            return (AppName: app.Name ?? "", ProgramName: programName ?? "");

        }
        private async Task<IEnumerable<AppInputSummarizedVm>> GetSummarizedAppInput(int appId, int nMonth, int nYear)
        {
            string sql = $@"SELECT a.Id
                                  ,a.CourtId
                                  ,a.MetricsFieldId
                                  ,a.Nmonth
                                  ,a.PlannedYear
                                  ,SUM(a.Nvalue) OVER (
                                      PARTITION BY a.CourtId, a.MetricsFieldId, a.PlannedYear, a.Nmonth
                                      ORDER BY a.MetricsFieldId
                                      ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
                                  ) AS CalculatedValue
                                  ,a.EnteredDate
                                  ,m.Code AS MetricsFieldCode
                                  ,m.Name AS MetricsFieldName
                                  ,c.Name AS CourtName
                            FROM AppInput a
                            LEFT JOIN MetricsField m ON a.MetricsFieldId = m.Id
                            LEFT JOIN Court c ON a.CourtId = c.Id
                            WHERE a.PlannedYear = @nYear AND a.Nmonth = @nMonth
                    AND a.CourtId IN (SELECT Id FROM Court WHERE CourtTypeId IN (
                SELECT Id FROM CourtType WHERE InstitutionTypeId IN(select InstitutionTypeId from AppRequired where AppId = @appId)
                               
                    )
                 )
                ORDER BY a.MetricsFieldId";
            var parameters = new Dictionary<string, object>
                {
                    { "@appId", appId },
                    { "@nYear", nYear },
                    { "@nMonth", nMonth }
                };
            return await _sjcService.QueryRawList<AppInputSummarizedVm>(sql, parameters);
        }
        private async Task<IEnumerable<AppInputSummarizedCommonVm>> GetSummarizedAppInputCommon(int appId, int nYear, int? modelReport)
        {
            string sql = string.Empty;
            if (modelReport == 1)
            {
                sql = $@"SELECT 
                        a.Id,
                        a.CourtId,
                        a.MetricsFieldId,
                        m.Code AS MetricsFieldCode,
                        m.Name AS MetricsFieldName,
                        a.PlannedYear ,
                        SUM(a.Nvalue) AS Nval1,              -- Base year
                        SUM(b.Nvalue) AS Nval2,              -- Base year + 1
                        SUM(c.Nvalue) AS Nval3,               -- Base year + 2
	                    SUM(d.Nvalue) AS Nval4               -- Base year + 3
                    FROM AppInputCourt a
                    LEFT JOIN AppInputCourt b 
                        ON a.CourtId = b.CourtId 
                        AND a.MetricsFieldId = b.MetricsFieldId 
                        AND b.PlannedYear = a.PlannedYear + 1
                    LEFT JOIN AppInputCourt c 
                        ON a.CourtId = c.CourtId 
                        AND a.MetricsFieldId = c.MetricsFieldId 
                        AND c.PlannedYear = a.PlannedYear + 2
                    LEFT JOIN AppInputCourt d 
                        ON a.CourtId = d.CourtId 
                        AND a.MetricsFieldId = d.MetricsFieldId 
                        AND d.PlannedYear = a.PlannedYear + 3
                    LEFT JOIN MetricsField m 
                        ON a.MetricsFieldId = m.Id
                    WHERE 
                        a.PlannedYear = @nYear
                        AND m.Id IN (
                            SELECT DISTINCT admf.MetricsFieldId
                            FROM AppDefMetricsField admf
                            WHERE admf.AppDefId IN (
                                SELECT ad.Id FROM AppDef ad WHERE ad.AppId = @appId
                            )
                        )
                    GROUP BY 
                        a.Id,
                        a.CourtId,
                        a.MetricsFieldId,
                        a.PlannedYear,
                        m.Code,
                        m.Name
                    ORDER BY 
                        a.Id,
                        a.CourtId,
                        m.Code;";
                var parameters = new Dictionary<string, object>
                {
                    { "@appId", appId },
                    { "@nYear", nYear }
                };
                return await _sjcService.QueryRawList<AppInputSummarizedCommonVm>(sql, parameters);
            }
            else
            {
                sql = $@"SELECT 
                        a.Id,
                        a.CreatedByInstTypeId as CourtId,
                        a.MetricsFieldId,
                        m.Code AS MetricsFieldCode,
                        m.Name AS MetricsFieldName,
                        a.PlannedYear ,
                        SUM(a.Nvalue) AS Nval1,              -- Base year
                        SUM(b.Nvalue) AS Nval2,              -- Base year + 1
                        SUM(c.Nvalue) AS Nval3,               -- Base year + 2
	                    SUM(d.Nvalue) AS Nval4               -- Base year + 3
                    FROM AppInputCommon a
                    LEFT JOIN AppInputCommon b 
                        ON a.CreatedByInstTypeId = b.CreatedByInstTypeId 
                        AND a.MetricsFieldId = b.MetricsFieldId 
                        AND b.PlannedYear = a.PlannedYear + 1
                    LEFT JOIN AppInputCommon c 
                        ON a.CreatedByInstTypeId = c.CreatedByInstTypeId 
                        AND a.MetricsFieldId = c.MetricsFieldId 
                        AND c.PlannedYear = a.PlannedYear + 2
                    LEFT JOIN AppInputCommon d 
                        ON a.CreatedByInstTypeId = d.CreatedByInstTypeId 
                        AND a.MetricsFieldId = d.MetricsFieldId 
                        AND d.PlannedYear = a.PlannedYear + 3
                    LEFT JOIN MetricsField m 
                        ON a.MetricsFieldId = m.Id
                    WHERE 
                        a.PlannedYear = @nYear
                        AND m.Id IN (
                            SELECT DISTINCT admf.MetricsFieldId
                            FROM AppDefMetricsField admf
                            WHERE admf.AppDefId IN (
                                SELECT ad.Id FROM AppDef ad WHERE ad.AppId = @appId
                            )
                        )
                    GROUP BY 
                        a.Id,
                        a.CreatedByInstTypeId,
                        a.MetricsFieldId,
                        a.PlannedYear,
                        m.Code,
                        m.Name
                    ORDER BY 
                        a.Id,
                        a.CreatedByInstTypeId,
                        m.Code;";
                var parameters = new Dictionary<string, object>
                {
                    { "@appId", appId },
                    { "@nYear", nYear }
                };
                return await _sjcService.QueryRawList<AppInputSummarizedCommonVm>(sql, parameters);
            }

        }
        private async Task<List<AppDefVm>> GetAppDefaultByAppId(int? appId)
        {
            try
            {
                var data = await _sjcService.QueryRawList<AppDefVm>($@"SELECT a.Id ,a.FunctionalSubAreaId ,a.AppId ,a.RowNum ,a.RowCode,a.Name,a.ParentRowNum,a.IsActive ,a.MeasureId,a.Formula,b.Name as AppName,c.Name as MeasureName
          FROM  dbo.AppDef a
         left join App b on a.appId=b.id
         left join Measure c on a.MeasureID=c.id
         where a.AppId={appId ?? 0}");
                return data.ToList();
            }
            catch (Exception ex)
            {
                return new List<AppDefVm>();
            }
        }
        public async Task<JsonResult> Index(string? par)
        {
            try
            {
                string[] args = par.Split('|');
                int.TryParse(args[0], out int appId);
                int.TryParse(args[1], out int nMonth);
                int.TryParse(args[2], out int nYear);
                string resultFile = Guid.NewGuid().ToString("N") + ".xlsx";
                string excelResultFilePath = System.IO.Path.Combine(_env.WebRootPath + $"/Temp/{resultFile}");
                string excelFile = System.IO.Path.Combine(_env.WebRootPath + $"/templates/{templateName}");
                var (appName, programName) = await GetProgramNameByAppId(appId);
                var appRequired = await _sjcService.QueryRawList<AppRequiredVm>($@"SELECT a.Id
                                  ,a.AppId
                                  ,a.InstitutionTypeId
                                  ,a.IsActive
	                              ,i.Name as InstitutionTypeName
                                   FROM AppRequired a
                                   left join InstitutionType i on a.InstitutionTypeId=i.id
                                   where a.AppId={appId}");
                var summarizedAppInput = await GetSummarizedAppInput(appId, nMonth, nYear);
                var appDef = await GetAppDefaultByAppId(appId);

                StringBuilder sb = new StringBuilder();
                using (var excelWorkbook = new XLWorkbook(excelFile))
                {
                    var worksheet = excelWorkbook.Worksheet(1);
                    worksheet.Cell("B2").SetValue(programName.Replace("\r\n", ""));
                    worksheet.Cell("E1").SetValue(appName);
                    worksheet.Cell("B3").Value = $"Период: месец {nMonth} година {nYear}";

                    int rowOffset = 5;
                    for (int i = 0; i < appDef.Count; i++)
                    {
                        AppDefVm item = appDef[i];
                        rowOffset += 1;
                        decimal? calculatedValue = await CalculateFormula(item.Formula, summarizedAppInput, nMonth, nYear, sb);
                        worksheet.Cell("B" + rowOffset).Value = item.RowCode;
                        worksheet.Cell("C" + rowOffset).Value = item.Name;
                        worksheet.Cell("D" + rowOffset).Value = item.MeasureName;
                        if (calculatedValue != null)
                        {
                            worksheet.Cell("E" + rowOffset).Value = calculatedValue;
                        }
                    }
                    excelWorkbook.SaveAs(excelResultFilePath);
                }
                //------------------------------------------------------------------------------
                var resultLogfile = $"calculation_{Guid.NewGuid().ToString("N")}.txt";
                string resultfilepath = System.IO.Path.Combine(_env.WebRootPath + "/Temp/", resultLogfile);
                using (StreamWriter writer = new StreamWriter(resultfilepath, false, Encoding.UTF8))
                {
                    writer.Write(sb.ToString());
                }
                return Json(new { success = true, msg = "ok", resultFile = resultFile, logFile = resultLogfile });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AppResultController.Index");
                return Json(new { success = false, msg = ex.Message });
            }
        }
        #region plannedYearsCommon
        public async Task<JsonResult> PlannedResult(string? par)
        {
            try
            {
                string[] args = par.Split('|');
                int.TryParse(args[0], out int appId);
                int.TryParse(args[1], out int nYear);
                int.TryParse(args[2], out int nModelReport);
                int nMonth = 12; // For common planned years, we assume the last month of the year
                string resultFile = Guid.NewGuid().ToString("N") + ".xlsx";
                string excelResultFilePath = System.IO.Path.Combine(_env.WebRootPath + $"/Temp/{resultFile}");
                string excelFile = System.IO.Path.Combine(_env.WebRootPath + $"/templates/{templateCommonName}");
                var (appName, programName) = await GetProgramNameByAppId(appId);
                var appRequired = await _sjcService.QueryRawList<AppRequiredVm>($@"SELECT a.Id
                                  ,a.AppId
                                  ,a.InstitutionTypeId
                                  ,a.IsActive
	                              ,i.Name as InstitutionTypeName
                                   FROM AppRequired a
                                   left join InstitutionType i on a.InstitutionTypeId=i.id
                                   where a.AppId={appId}");
                var summarizedAppInput = await GetSummarizedAppInputCommon(appId, nYear, nModelReport);
                var appDef = await GetAppDefaultByAppId(appId);
                var activeYears = await _sjcServiceV2.GetActiveBudgetPeriodAsync();
                int[] activeYearsArray = new int[] { activeYears?.Y1 ?? 0, activeYears?.Y2 ?? 0, activeYears?.Y3 ?? 0, activeYears?.Y4 ?? 0 };
                StringBuilder sb = new StringBuilder();
                using (var excelWorkbook = new XLWorkbook(excelFile))
                {
                    var worksheet = excelWorkbook.Worksheet(1);
                    worksheet.Cell("B2").Value = programName.Replace("\r\n", "");
                    worksheet.Cell("B1").Value = appName;
                    worksheet.Cell("B3").Value = $"Целева стойност";
                    worksheet.Cell("E5").Value = $"Прогноза {activeYears?.Y1 ?? 0} г.";
                    worksheet.Cell("F5").Value = $"Прогноза {activeYears?.Y2 ?? 0} г.";
                    worksheet.Cell("G5").Value = $"Прогноза {activeYears?.Y3 ?? 0} г.";
                    worksheet.Cell("H5").Value = $"Прогноза {activeYears?.Y4 ?? 0} г.";
                    int rowOffset = 5;
                    for (int i = 0; i < appDef.Count; i++)
                    {
                        AppDefVm item = appDef[i];
                        rowOffset += 1;
                        var rez = await CalculateFormulaCommon(item.Formula, summarizedAppInput, nMonth, activeYearsArray, sb);
                        decimal?[] rezArray = rez.ToArray();
                        if (rezArray.Length == 4)
                        {


                            worksheet.Cell("B" + rowOffset).Value = item.RowCode;
                            worksheet.Cell("C" + rowOffset).Value = item.Name;
                            worksheet.Cell("D" + rowOffset).Value = item.MeasureName;
                            if (rezArray[0] != null)
                            {
                                worksheet.Cell("E" + rowOffset).Value = (decimal)rezArray[0];
                            }
                            if (rezArray[1] != null)
                            {
                                worksheet.Cell("F" + rowOffset).Value = (decimal)rezArray[1];
                            }
                            if (rezArray[2] != null)
                            {
                                worksheet.Cell("G" + rowOffset).Value = (decimal)rezArray[2];
                            }
                            if (rezArray[3] != null)
                            {
                                worksheet.Cell("H" + rowOffset).Value = (decimal)rezArray[3];
                            }
                        }
                    }
                    excelWorkbook.SaveAs(excelResultFilePath);
                }
                //------------------------------------------------------------------------------
                var resultLogfile = $"calculation_{Guid.NewGuid().ToString("N")}.txt";
                string resultfilepath = System.IO.Path.Combine(_env.WebRootPath + "/Temp/", resultLogfile);
                using (StreamWriter writer = new StreamWriter(resultfilepath, false, Encoding.UTF8))
                {
                    writer.Write(sb.ToString());
                }
                return Json(new { success = true, msg = "ok", resultFile = resultFile, logFile = resultLogfile });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AppResultController.Index");
                return Json(new { success = false, msg = ex.Message });
            }
        }
        #endregion
        private async Task<decimal?> CalculateFormula(string formula, IEnumerable<AppInputSummarizedVm> appInputs, int nMonth, int nYear, StringBuilder sb)
        {
            var parameters = Toolbox.ExtractCalcArgs(formula);
            sb.AppendLine($"Formula:{formula}, Parameters: {string.Join(", ", parameters)}");
            var dic = new Dictionary<string, string>();
            foreach (var item in parameters)
            {
                if (item?.ToUpper() == "TMONTHS")
                {
                    dic.Add(item, $"{nMonth}");
                }

            }
            var resultForCalc = appInputs
               .Where(item => parameters.Contains(item.MetricsFieldCode, StringComparer.OrdinalIgnoreCase))
               .GroupBy(item => item.MetricsFieldCode)
               .Select(g => new
               {
                   Code = g.Key,
                   Value = g.Sum(x => x.CalculatedValue)
               })
               .ToList();
            if (resultForCalc.Any())
            {
                foreach (var item in resultForCalc)
                {
                    if (!dic.ContainsKey(item.Code))
                    {
                        dic.Add(item.Code, item.Value?.ToString() ?? "0");
                    }
                }
                string calculationString = ReplaceCalculationFormula(formula ?? string.Empty, dic);
                sb.AppendLine($"Calculation String: {calculationString}");
                if (ValidateString(calculationString))
                {
                    try
                    {
                        var res = Parser.Parse(calculationString).Eval(null);
                        bool isNaN = Double.IsNaN(res);
                        if (isNaN) res = 0;
                        sb.AppendLine($"Calculation Result: {res}");
                        return (decimal?)Math.Round(res, 2, MidpointRounding.AwayFromZero);

                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"Error in calculation: {ex?.Message}");
                        return null;
                    }
                }
            }
            return null;
        }
        private async Task<List<decimal?>> CalculateFormulaCommon(string formula, IEnumerable<AppInputSummarizedCommonVm> appInputs, int nMonth, int[] activeYearsAr, StringBuilder sb)
        {
            try
            {

                var returnValues = new List<decimal?>();
                var parameters = Toolbox.ExtractCalcArgs(formula);
                sb.AppendLine($"Formula:{formula}, Parameters: {string.Join(", ", parameters)}");
                var dic1 = new Dictionary<string, string>();
                var dic2 = new Dictionary<string, string>();
                var dic3 = new Dictionary<string, string>();
                var dic4 = new Dictionary<string, string>();
                foreach (var item in parameters)
                {
                    if (item?.ToUpper() == "TMONTHS")
                    {
                        dic1.Add(item, $"{nMonth}");
                        dic2.Add(item, $"{nMonth}");
                        dic3.Add(item, $"{nMonth}");
                        dic4.Add(item, $"{nMonth}");
                    }

                }
                var resultForCalc1 = appInputs
                   .Where(item => parameters.Contains(item.MetricsFieldCode, StringComparer.OrdinalIgnoreCase))
                   .GroupBy(item => item.MetricsFieldCode)
                   .Select(g => new
                   {
                       Code = g.Key,
                       Value = g.Sum(x => x.Nval1)
                   })
                   .ToList();
                if (resultForCalc1.Any())
                {
                    foreach (var item in resultForCalc1)
                    {
                        if (!dic1.ContainsKey(item.Code))
                        {
                            dic1.Add(item.Code, item.Value?.ToString() ?? "0");
                        }
                    }
                    string calculationString = ReplaceCalculationFormula(formula ?? string.Empty, dic1);
                    sb.AppendLine($"Calculation String: {calculationString}");
                    if (ValidateString(calculationString))
                    {
                        try
                        {
                            var res = Parser.Parse(calculationString).Eval(null);
                            bool isNaN = Double.IsNaN(res);
                            if (isNaN) res = 0;
                            sb.AppendLine($"Calculation Result: {res}");
                            returnValues.Add((decimal?)Math.Round(res, 2, MidpointRounding.AwayFromZero));

                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"Error in calculation: {ex?.Message}");
                            return new List<decimal?>();
                        }
                    }
                }
                //-------result 2
                var resultForCalc2 = appInputs
                  .Where(item => parameters.Contains(item.MetricsFieldCode, StringComparer.OrdinalIgnoreCase))
                  .GroupBy(item => item.MetricsFieldCode)
                  .Select(g => new
                  {
                      Code = g.Key,
                      Value = g.Sum(x => x.Nval2)
                  })
                  .ToList();
                if (resultForCalc2.Any())
                {
                    foreach (var item in resultForCalc2)
                    {
                        if (!dic2.ContainsKey(item.Code))
                        {
                            dic2.Add(item.Code, item.Value?.ToString() ?? "0");
                        }
                    }
                    string calculationString = ReplaceCalculationFormula(formula ?? string.Empty, dic2);
                    sb.AppendLine($"Calculation String: {calculationString}");
                    if (ValidateString(calculationString))
                    {
                        try
                        {
                            var res = Parser.Parse(calculationString).Eval(null);
                            bool isNaN = Double.IsNaN(res);
                            if (isNaN) res = 0;
                            sb.AppendLine($"Calculation Result: {res}");
                            returnValues.Add((decimal?)Math.Round(res, 2, MidpointRounding.AwayFromZero));

                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"Error in calculation: {ex?.Message}");
                            return new List<decimal?>();
                        }
                    }
                }
                //-------result 3
                var resultForCalc3 = appInputs
                  .Where(item => parameters.Contains(item.MetricsFieldCode, StringComparer.OrdinalIgnoreCase))
                  .GroupBy(item => item.MetricsFieldCode)
                  .Select(g => new
                  {
                      Code = g.Key,
                      Value = g.Sum(x => x.Nval3)
                  })
                  .ToList();
                if (resultForCalc3.Any())
                {
                    foreach (var item in resultForCalc3)
                    {
                        if (!dic3.ContainsKey(item.Code))
                        {
                            dic3.Add(item.Code, item.Value?.ToString() ?? "0");
                        }
                    }
                    string calculationString = ReplaceCalculationFormula(formula ?? string.Empty, dic3);
                    sb.AppendLine($"Calculation String: {calculationString}");
                    if (ValidateString(calculationString))
                    {
                        try
                        {
                            var res = Parser.Parse(calculationString).Eval(null);
                            bool isNaN = Double.IsNaN(res);
                            if (isNaN) res = 0;
                            sb.AppendLine($"Calculation Result: {res}");
                            returnValues.Add((decimal?)Math.Round(res, 2, MidpointRounding.AwayFromZero));

                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"Error in calculation: {ex?.Message}");
                            return new List<decimal?>();
                        }
                    }
                }
                //-------result 4
                var resultForCalc4 = appInputs
                  .Where(item => parameters.Contains(item.MetricsFieldCode, StringComparer.OrdinalIgnoreCase))
                  .GroupBy(item => item.MetricsFieldCode)
                  .Select(g => new
                  {
                      Code = g.Key,
                      Value = g.Sum(x => x.Nval4)
                  })
                  .ToList();
                if (resultForCalc4.Any())
                {
                    foreach (var item in resultForCalc4)
                    {
                        if (!dic4.ContainsKey(item.Code))
                        {
                            dic4.Add(item.Code, item.Value?.ToString() ?? "0");
                        }
                    }
                    string calculationString = ReplaceCalculationFormula(formula ?? string.Empty, dic4);
                    sb.AppendLine($"Calculation String: {calculationString}");
                    if (ValidateString(calculationString))
                    {
                        try
                        {
                            var res = Parser.Parse(calculationString).Eval(null);
                            bool isNaN = Double.IsNaN(res);
                            if (isNaN) res = 0;
                            sb.AppendLine($"Calculation Result: {res}");
                            returnValues.Add((decimal?)Math.Round(res, 2, MidpointRounding.AwayFromZero));

                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"Error in calculation: {ex?.Message}");
                            return new List<decimal?>();
                        }
                    }
                }
                return returnValues;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error in CalculateFormulaCommon: {ex?.Message}");
                return new List<decimal?>();
            }
        }
        private string ReplaceCalculationFormula(string Source, Dictionary<string, string> dic)
        {
            if (string.IsNullOrWhiteSpace(Source)) return Source;

            // Find all unique tokens (words) in formula
            var tokens = Regex.Matches(Source, @"\b\w+\b")
                              .Cast<Match>()
                              .Select(m => m.Value)
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .ToList();

            foreach (var token in tokens)
            {
                string replacement = "0";

                // If token contains only digits, use it directly
                if (Regex.IsMatch(token, @"^\d+$"))
                {
                    replacement = token;
                }
                else
                {
                    // Otherwise, look in dictionary
                    if (dic.TryGetValue(token, out var val) &&
                        !string.IsNullOrWhiteSpace(val) &&
                        decimal.TryParse(val, out var number) &&
                        number != 0)
                    {
                        replacement = val;
                    }
                }

                var pattern = $@"\b{Regex.Escape(token)}\b";
                Source = Regex.Replace(Source, pattern, replacement, RegexOptions.IgnoreCase);
            }

            return Source;
        }
        private bool ValidateString(string s)
        {
            char[] enabledcharc = new char[] { '*', '/', '+', '-', '(', ')', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ', '.' };
            foreach (var c in s)
            {
                if (!enabledcharc.Contains(c))
                {
                    return false;
                }
            }
            return true;
        }
        public async Task<JsonResult> GenerateCourtStatistics(string par)
        {
            try
            {
                string[] args = par.Split('|');
                int.TryParse(args[0], out int appId);
                int.TryParse(args[1], out int nYear);
              
                int nMonth = 12; // For common planned years, we assume the last month of the year
                string resultFile = Guid.NewGuid().ToString("N") + ".xlsx";
                string excelResultFilePath = System.IO.Path.Combine(_env.WebRootPath + $"/Temp/{resultFile}");
                string excelFile = System.IO.Path.Combine(_env.WebRootPath + $"/templates/AppCourtsCheck.xlsx");
                var (appName, programName) = await GetProgramNameByAppId(appId);
                var activeYears = await _sjcServiceV2.GetActiveBudgetPeriodAsync();
                var appRequired = await _sjcService.QueryRawList<IdName>($@"SELECT c.Id,concat(c.Name,'  (',i.Name,')')  as Name
                                  
	                              
                                   FROM AppRequired a
                                   left join InstitutionType i on a.InstitutionTypeId=i.id
								   left join CourtType t on i.Id=t.InstitutionTypeId
								   left join Court c on t.id=c.CourtTypeId
                                   where a.AppId={appId}");
                var filledCourts = await _sjcService.QueryRawList<int>($@"SELECT distinct courtId from AppInputCourt where appID={appId} and PlannedYear between {activeYears.Y1} and {activeYears?.Y4}");
                var bIDs = new HashSet<int>(filledCourts);
                var result = appRequired.Select(x => $"{x.Name} - {(bIDs.Contains(x.Id) ? "Подадени данни" : "Не са подадени данни")}")
               .ToList();
              
                

                using (var excelWorkbook = new XLWorkbook(excelFile))
                {
                    var worksheet = excelWorkbook.Worksheet(1);
                    worksheet.Cell("C4").Value = appName;
                    
                    int rowOffset = 5;
                    for (int i = 0; i < result.Count; i++)
                    {
                        string item = result[i];
                        rowOffset += 1;

                            worksheet.Cell("C" + rowOffset).Value = item;
                        
                    }
                    excelWorkbook.SaveAs(excelResultFilePath);
                }
              
                return Json(new { success = true, msg = "ok", resultFile = resultFile });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AppResultController.Index");
                return Json(new { success = false, msg = ex.Message });
            }
        }
    }
}
