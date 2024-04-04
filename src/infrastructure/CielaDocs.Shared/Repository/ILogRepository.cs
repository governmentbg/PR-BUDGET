using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Shared.Repository
{
    public interface ILogRepository
    {
        Task AddToUserLogAsync(Ulog data);
        Task AddToAppUserLogAsync(AppUserLog data);
        Task<IEnumerable<Ulog>> GetUserLogByOnrAsync(DateTime startDate, DateTime endDate,int onrId);
        Task<IEnumerable<Ulog>> GetUserLogByEmplAsync(DateTime startDate, DateTime endDate,int emplId);
        Task<IEnumerable<Ulog>> GetUserLogByCardAsync(DateTime startDate, DateTime endDate, long cardId);
        Task<IEnumerable<Ulog>> GetUserLogByOnrAsync(string sql);
        Task<IEnumerable<Ulog>> GetUserLogByEmplAsync(string sql);
        Task<IEnumerable<Ulog>> GetUserLogByCardAsync(string sql);
        Task<IEnumerable<AppUserLog>> GetAppUserLogByUserAsync(string sql);
        
        Task<IEnumerable<AppUserLog>> GetAppUserLogByAppUserIdAsync(int appUserId);
        Task<IEnumerable<AppUserLog>> GetAppUserLogAsync(DateTime startDate, DateTime endDate);
    }
}
