using Dapper;

using CielaDocs.Domain.Entities;
using CielaDocs.Shared.DataAccess;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using CielaDocs.Application.Models;

namespace CielaDocs.Shared.Repository
{
    public class LogRepository : ILogRepository
    {
        private LogContext _context;

        public LogRepository(LogContext context)
        {
            this._context = context;
        }
        public async Task AddToUserLogAsync(Ulog data)
        {

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
                var sqlStatement = @"INSERT INTO [dbo].[ULog]([OnrId],[EmplId],[CardId],[MsgId],[Msg],[IP],[CreatedOn]) VALUES(@OnrId,@EmplId,@CardId,@MsgId,@Msg,@IP,getdate())";
                await connection.ExecuteAsync(sqlStatement, data);
        }
        public async Task AddToAppUserLogAsync(AppUserLog data)
        {

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var sqlStatement = @"INSERT INTO [dbo].[AppUserLog]([AppUserId],[MsgId],[Msg],[IP],[CreatedOn]) VALUES(@AppUserId,@MsgId,@Msg,@IP,getdate())";
            await connection.ExecuteAsync(sqlStatement, data);
        }

        public async Task<IEnumerable<Ulog>> GetUserLogByEmplAsync(DateTime startDate, DateTime endDate,int emplId )
        {
            var sql = "SELECT * FROM Ulog WHERE CreatedOn>=@StartDate and CreatedOn<=@endDate and EmplId = @emplId";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<Ulog>(sql, new { StartDate = startDate, EndDate = endDate, emplId = emplId });
            return result;
        }

        public async Task<IEnumerable<Ulog>> GetUserLogByEmplAsync(string sql)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<Ulog>(sql);
            return result;
        }

        public async Task<IEnumerable<Ulog>> GetUserLogByOnrAsync(DateTime startDate, DateTime endDate,int onrId)
        {
            var sql = "SELECT * FROM Ulog WHERE CreatedOn>=@StartDate and CreatedOn<=@EndDate and OnrId = @OnrId";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<Ulog>(sql, new {StartDate=startDate,EndDate=endDate, OnrId = onrId });
            return result;
            
        }

        public async Task<IEnumerable<Ulog>> GetUserLogByOnrAsync(string sql)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<Ulog>(sql);
            return result;
        }
        public async Task<IEnumerable<Ulog>> GetUserLogByCardAsync(DateTime startDate, DateTime endDate, long cardId)
        {
            var sql = "SELECT * FROM Ulog WHERE CreatedOn>=@StartDate and CreatedOn<=@EndDate and CardId = @CardId";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<Ulog>(sql, new { StartDate = startDate, EndDate = endDate, CardId = cardId });
            return result;

        }
        public async Task<IEnumerable<ApplicationLogVm>> GetApplicationLogAsync() {
            var sql = "SELECT top 10000 u.id,u.EmplId,u.msg,u.ip,u.CreatedOn,concat(e.FirstName,space(1),e.MiddleName,SPACE(1),e.LastName) as Fullname  FROM SjcBudgetLog.dbo.Ulog u left join SjcBudget.Dbo.Users e on u.EmplId=e.Id order by u.createdOn desc";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ApplicationLogVm>(sql);
            return result;
        }
        public async Task<IEnumerable<Ulog>> GetUserLogByCardAsync(string sql)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<Ulog>(sql);
            return result;
        }
        public async Task<IEnumerable<AppUserLog>> GetAppUserLogByUserAsync(string sql)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<AppUserLog>(sql);
            return result;
        }

        public async Task<IEnumerable<AppUserLog>> GetAppUserLogByAppUserIdAsync(int appUserId)
        {
            var sql = "SELECT * FROM AppUserLog WHERE AppUserId=@AppUserId";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<AppUserLog>(sql, new { AppUserId= appUserId });
            return result;
        }
        public async Task<IEnumerable<AppUserLog>> GetAppUserLogAsync(DateTime startDate, DateTime endDate)
        {
            var sql = "SELECT * FROM AppUserLog WHERE CreatedOn>=@StartDate and CreatedOn<=@EndDate";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<AppUserLog>(sql, new { StartDate = startDate, EndDate = endDate });
            return result;

        }
    }
}
