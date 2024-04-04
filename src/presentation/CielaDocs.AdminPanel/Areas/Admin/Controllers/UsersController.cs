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
using DocumentFormat.OpenXml.Office2010.Excel;

namespace CielaDocs.AdminPanel.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : Controller
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IMediator _mediator;
            private readonly IMapper _mapper;
            private readonly ILogRepository _logRepo;
            private readonly ISjcBudgetRepository _sjcRepo;
            private readonly IHttpContextAccessor _httpContextAccessor;

            public UsersController(SignInManager<IdentityUser> signInManager,
             ILogger<UsersController> logger,
             UserManager<IdentityUser> userManager, IMediator mediator, IMapper mapper, ILogRepository logRepo, ISjcBudgetRepository sjcRepo, IHttpContextAccessor httpContextAccessor)
            {
            _userManager = userManager;
            _signInManager = signInManager;
            _mediator = mediator;
                _mapper = mapper;
                _logRepo = logRepo;
                _sjcRepo = sjcRepo;
                _httpContextAccessor = httpContextAccessor;
            }

            public async Task<IActionResult> Index()
            {
                var user = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });


                ViewBag.UserId = user?.Id ?? 0;
                var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                string logmsg = $"Достъп до потребители на системата от {User?.Identity?.Name}";
                await _logRepo.AddToAppUserLogAsync(new Domain.Entities.AppUserLog { AppUserId = user?.Id ?? 0, MsgId = 0, Msg = logmsg, IP = ip });
                return View();
            }
        [HttpGet]
        public async Task<JsonResult> GetAllUsersByCourtTypeId(int? courtTypeId)
        {
            try
            {
                var data = await _sjcRepo.GetUsersByCourtTypeIdAsync(courtTypeId ?? 0);
                return Json(data.ToList());
            }
            catch (Exception ex)
            {
                return Json(new List<CourtsVm>());
            }
        }
        [HttpGet]
            public PartialViewResult AddUserPartial()
            {

                return PartialView("AddUserPartialView", new UserDto { Id = 0 ,UserTypeId=0, CourtId=0 });


            }
            [HttpGet]
            public async Task<PartialViewResult> EditUserPartial(int? id)
            {
                if ((id == null) || (id < 0))
                {
                    return PartialView("_ErrorPartialView", "Невалиден указател към потребител!");
                }
                var usr = await _mediator.Send(new GetUserByIdQuery { Id = id ?? 0 });
            if (string.IsNullOrWhiteSpace(usr.Email)) usr.Email = usr.UserName;

                return PartialView("AddUserPartialView", usr);
            }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateOnlyAspneUsers() {
            var dbusers = await _sjcRepo.GetUsersWithoutAspNetUserIdAsync();
            string aspNetUserId=string.Empty;
            if (dbusers.Any()) { 
                foreach (var dbuser in dbusers)
                {
                    var uexists = await _userManager.FindByEmailAsync(dbuser?.Email);
                    if (uexists == null)
                    {
                        var user = new IdentityUser { UserName = dbuser?.Email, Email = dbuser?.Email };
                        var result = await _userManager.CreateAsync(user, "Qwerty1!");
                        if (result.Succeeded)
                        {
                            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            var res = await _userManager.ConfirmEmailAsync(user, code);
                            aspNetUserId = user.Id;
                        }
                    }
                    else aspNetUserId=uexists.Id;
                    _ = await _sjcRepo.UpdateUserWithAspNetUserIdAsync(dbuser?.Id??0, aspNetUserId);
                }
            }
            return Json(new { msg = "Данните са заредени ", success = true, id = 0 });
        }
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<JsonResult> SaveUser(UserDto record)
            {
                 string aspNetUserId = string.Empty;
                if (ModelState.IsValid)
                {

                   
                    var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
                if ((!empl.CanAdd) && (!empl.CanUpdate))
                {
                    return Json(new { msg = "Нямате предоставени права да добавяте/редактирате данни ", success = false, id = 0 });
                }
                if (record.Id==0)
                    {

                        try
                        {
                            List<CustErrors> errLst = GetCustErrors(record);
                            if (errLst.Count > 0)
                            {
                                string messages = string.Join(";", errLst.Select(x => x.Name));
                                return Json(new { msg = messages, success = false, id = 0 });
                            }
                            if (!string.IsNullOrWhiteSpace(record?.Password))
                            {

                            var uexists= await _userManager.FindByEmailAsync(record.UserName);
                            if (uexists!=null)
                            {
                                return Json(new { msg = "Използвайте друг имейл за потребителя", success = false, id = 0 });
                            }
                                var user = new IdentityUser { UserName = record.UserName, Email = record.UserName };
                                var result = await _userManager.CreateAsync(user, record.Password);
                                if (result.Succeeded)
                                {
                                    string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                                    var res = await _userManager.ConfirmEmailAsync(user, code);
                                    aspNetUserId=user.Id;
                                }
                            else return Json(new { msg = "Грешка при регистриране на нов потребител ", success = false, id = 0 });
                        }





                            CreateUserCommand command = new CreateUserCommand
                            {
                                    FirstName = record?.FirstName ?? String.Empty,
                                    MiddleName = record?.MiddleName ?? String.Empty,
                                    LastName = record?.LastName ?? String.Empty,
                                    Identifier = record?.Identifier ?? String.Empty,
                                    Email = record?.UserName ?? String.Empty,
                                    UserName = record?.UserName ?? String.Empty,
                                    LoginEnabled = record?.LoginEnabled ?? false,
                                    UserTypeId = record?.UserTypeId ?? 0,
                                    CourtId = record?.CourtId ?? 0,
                                    AspNetUserId = aspNetUserId,
                                    CanAdd = record?.CanAdd ?? false,
                                    CanUpdate = record?.CanUpdate ?? false,
                                    CanDelete = record?.CanDelete ?? false,
                            };
                            var ret = await _mediator.Send(command);

                            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                            string logmsg = $"Добавен бе нов потребител {record?.UserName} от {empl?.UserName}";
                            await _logRepo.AddToUserLogAsync(new Domain.Entities.Ulog { OnrId = record?.Id ?? 0, EmplId = empl?.Id ?? 0, CardId = 0, MsgId = (int?)CommonConstants.LogMessageType.Add, Msg = logmsg, IP = ip });

                            return Json(new { msg = "Добавен бе нов потребител ", success = true, id = ret });
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

                        UpdateUserCommand command = new UpdateUserCommand
                        {
                            Id= record?.Id ?? 0,
                            FirstName = record?.FirstName ?? String.Empty,
                            MiddleName = record?.MiddleName ?? String.Empty,
                            LastName = record?.LastName ?? String.Empty,
                            Identifier = record?.Identifier ?? String.Empty,
                            Email = record?.UserName ?? String.Empty,
                            UserTypeId = record?.UserTypeId ?? 0,
                            CourtId = record?.CourtId ?? 0,
                            LoginEnabled = record?.LoginEnabled ?? false,
                            CanAdd = record?.CanAdd ?? false,
                            CanUpdate = record?.CanUpdate ?? false,
                            CanDelete = record?.CanDelete ?? false,

                        };
                        var ret = await _mediator.Send(command);
                        var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                        string logmsg = $"Редактирани данни за потребител {record?.UserName} от {empl?.UserName}";
                        await _logRepo.AddToUserLogAsync(new Domain.Entities.Ulog { OnrId = record?.Id ?? 0, EmplId = empl?.Id ?? 0, CardId = 0, MsgId = (int?)CommonConstants.LogMessageType.Edit, Msg = logmsg, IP = ip });

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
            List<CustErrors> GetCustErrors(UserDto data)
            {

                List<CustErrors> ret = new List<CustErrors>();

            if (string.IsNullOrWhiteSpace(data?.UserName))
            {
                ret.Add(new CustErrors { Name = "Невъведен имейл за потребителя!" });
            }
            else if (!Toolbox.IsValidEmail(data?.UserName ?? string.Empty)) {
                ret.Add(new CustErrors { Name = "Невалиден имейл адрес за потребителя!" });
            }
            if (string.IsNullOrWhiteSpace(data?.Password))
            {
                ret.Add(new CustErrors { Name = "Невъведена парола за потребителя!" });
            }

            if (data?.UserTypeId < -1)
                {
                    ret.Add(new CustErrors { Name = "Изберете вид на потребителя!" });
                }
                return ret;
            }
        [HttpPost]
        public async Task<ActionResult> DeleteUser(int id)
        {
            //TODO:analize cards->SigId

            var empl = await _mediator.Send(new GetUserByAspNetUserIdQuery { AspNetUserId = User.GetUserIdValue() });
            if (empl?.CanDelete != true)
            {
                return Json(new { result = false, msg = CommonConstants.LogMsgForbiddenDel });
            }

            bool ok = await _mediator.Send(new DeleteUserCommand { Id = id });
            if (!ok)
            {
                return Json(new { result = false, msg = "Изтриването бе неуспешно!" });
            }


            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            string logmsg = $"Изтриване на потребител с Id: {id} от {empl?.UserName}";
            await _logRepo.AddToUserLogAsync(new Domain.Entities.Ulog { OnrId = empl?.CourtId ?? 0, EmplId = empl.Id, CardId = 0, MsgId = (int?)CommonConstants.LogMessageType.Delete, Msg = logmsg, IP = ip });
            return Json(new { result = true, msg = "Записът бе премахнат" });
        }
    }
}
