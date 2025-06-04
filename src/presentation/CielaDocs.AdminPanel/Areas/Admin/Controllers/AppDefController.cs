using AutoMapper;

using CielaDocs.Application;
using CielaDocs.Application.Common.Constants;
using CielaDocs.Application.Dtos;
using CielaDocs.Shared.Repository;
using CielaDocs.AdminPanel.Extensions;
using CielaDocs.AdminPanel.Models;





using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CielaDocs.Application.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Graph;
using CielaDocs.Domain.Entities;
using CielaDocs.Shared.Services;

namespace CielaDocs.AdminPanel.Areas.Admin.Controllers
{
    [Area("admin")]
    public class AppDefController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogRepository _logRepo;
        private readonly ISjcBudgetRepository _sjcRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISjcService _sjcService;

        public AppDefController(IMediator mediator, IMapper mapper, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IHttpContextAccessor httpContextAccessor,ISjcService sjcService)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logRepo = logRepo;
            _sjcRepo = sjcRepo;
            _httpContextAccessor = httpContextAccessor;
            _sjcService = sjcService;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<PartialViewResult> AddAppDefPartial(int? functionalSubAreaId)
        {
            var functionalSubAreaName = await _mediator.Send(new GetFnSubAreaByIdQuery { Id = functionalSubAreaId ?? 0 });
            ViewBag.FnSubName = functionalSubAreaName?.Name ?? string.Empty;
            return PartialView("AddAppDefPartialView", new AppDefVm { Id = 0, FunctionalSubAreaId = functionalSubAreaId ?? 0, AppId=0 });


        }
        [HttpGet]
        public async Task<PartialViewResult> EditAppDefPartial(int? id)
        {
            if ((id == null) || (id < 0))
            {
                return PartialView("_ErrorPartialView", "Невалиден указател!");
            }
            var mi = await _sjcService.QueryRaw<AppDefVm>("SELECT * FROM AppDef WHERE Id=@Id", new { Id = id });
            var functionalSubAreaName = await _mediator.Send(new GetFnSubAreaByIdQuery { Id = mi?.FunctionalSubAreaId ?? 0 });
            ViewBag.FnSubName = functionalSubAreaName?.Name ?? string.Empty;
            return PartialView("AddAppDefPartialView", mi);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(AppDefVm record)
        {

            if (ModelState.IsValid)
            {
                var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
                if ((!empl.CanAdd) && (!empl.CanUpdate))
                {
                    return Json(new { msg = "Нямате предоставени права да добавяте/редактирате данни ", success = false, id = 0 });
                }

                if (record?.Id == 0)
                {

                    try
                    {
                        List<CustErrors> errLst = GetCustErrors(record);
                        if (errLst.Count > 0)
                        {
                            string messages = string.Join(";", errLst.Select(x => x.Name));
                            return Json(new { msg = messages, success = false, id = 0 });
                        }


                        string sql=$@"INSERT INTO [dbo].[AppDef]
                                   ([FunctionalSubAreaId]
                                   ,[AppId]
                                   ,[RowNum]
                                   ,[RowCode]
                                   ,[Name]
                                   ,[ParentRowNum]
                                   ,[IsActive]
                                   ,[MeasureId]
                                   ,[Formula])
                                OUTPUT INSERTED.ID
                                VALUES
                                   (@FunctionalSubAreaId
                                   ,@AppId
                                   ,@RowNum
                                   ,@RowCode
                                   ,@Name
                                   ,@ParentRowNum
                                   ,@IsActive
                                   ,@MeasureId
                                   ,@Formula)";

                     int newId  =await _sjcService.ExecuteRawScalarSql(sql,new 
                       
                        {
                            FunctionalSubAreaId = record?.FunctionalSubAreaId ?? 0,
                          AppId=record?.AppId ?? 0,
                            RowNum = record?.RowNum ?? 0,
                            RowCode= record?.RowCode ?? string.Empty,
                            Name = record?.Name,
                           
                            ParentRowNum=record?.ParentRowNum ?? 0,
                            IsActive = record?.IsActive ?? false,
                            MeasureId = record?.MeasureId ?? 0,
                            Formula = record?.Formula ?? string.Empty,
                           
                        });
                       

                        var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                        string logmsg = $"Добавен бе нов показател {record?.Name} от {User.GetUserName()}";
                        await _logRepo.AddToUserLogAsync(new Domain.Entities.Ulog { OnrId = record?.Id ?? 0, EmplId = User.GetEmplIdValue(), CardId = 0, MsgId = (int?)CommonConstants.LogMessageType.Add, Msg = logmsg, IP = ip });

                        return Json(new { msg = "Добавен бе нов орган на съдебна власт ", success = true, id = newId });
                    }
                    catch (Exception ex)
                    {
                        string messages = string.Join("; ", ModelState.Values
                             .SelectMany(x => x.Errors)
                             .Select(x => x.ErrorMessage));
                        return Json(new { msg = messages, success = false, id = 0 });
                    }

                }
                else
                {
                    string sqlupd = $@"UPDATE [dbo].[AppDef]
                       SET [FunctionalSubAreaId] = @FunctionalSubAreaId
                          ,[AppId] = @AppId
                          ,[RowNum] = @RowNum
                          ,[RowCode] = @RowCode
                          ,[Name] = @Name
                          ,[ParentRowNum] = @ParentRowNum
                          ,[IsActive] = @IsActive
                          ,[MeasureId] = @MeasureId
                          ,[Formula] = @Formula
                     WHERE ID=@ID";

                    _=await _sjcService.ExecuteRawSql(sqlupd,new
                    {
                        FunctionalSubAreaId = record?.FunctionalSubAreaId ?? 0,
                        AppId = record?.AppId ?? 0,
                        RowNum = record?.RowNum ?? 0,
                        RowCode = record?.RowCode ?? string.Empty,
                        Name = record?.Name,

                        ParentRowNum = record?.ParentRowNum ?? 0,
                        IsActive = record?.IsActive ?? false,
                        MeasureId = record?.MeasureId ?? 0,
                        Formula = record?.Formula ?? string.Empty,
                        ID=record?.Id ?? 0
                    });
                    
                    
                    var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                    string logmsg = $"Редактирани данни за показател {record?.Name} от {User.GetUserName()}";
                    await _logRepo.AddToUserLogAsync(new Domain.Entities.Ulog { OnrId = record?.Id ?? 0, EmplId = User.GetEmplIdValue(), CardId = 0, MsgId = (int?)CommonConstants.LogMessageType.Edit, Msg = logmsg, IP = ip });

                    return Json(new { msg = "Данните бяха редактирани", success = true });
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
        List<CustErrors> GetCustErrors(AppDefVm data)
        {

            List<CustErrors> ret = new List<CustErrors>();

            if (string.IsNullOrWhiteSpace(data?.Name))
            {
                ret.Add(new CustErrors { Name = "Невъведено име на показателя!" });
            }
            if (data?.FunctionalSubAreaId < -1)
            {
                ret.Add(new CustErrors { Name = "Изберете програма за показателя!" });
            }
           
            if (data?.MeasureId < -1)
            {
                ret.Add(new CustErrors { Name = "Изберете мярка на показателя!" });
            }
            return ret;
        }



    }
}
