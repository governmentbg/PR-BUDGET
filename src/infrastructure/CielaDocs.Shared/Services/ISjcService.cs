using CielaDocs.Application.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Shared.Services
{
    public interface ISjcService
    {
        Task<CfgVm> GetCfg();
        Task<IEnumerable<UserLockedItemVm>> GetAllUserLockedItems();
        Task<int> ExecuteRawSql(string sql);
        Task<IEnumerable<T>> QueryRawList<T>(string sql);
        Task<T> QueryRaw<T>(string sql);
    }
}
