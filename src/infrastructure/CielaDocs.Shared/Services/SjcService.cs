using CielaDocs.Application.Models;
using CielaDocs.Shared.Repository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Shared.Services
{
    public class SjcService:ISjcService
    {
        private readonly ISjcBudgetRepository _repo;

        public SjcService(ISjcBudgetRepository budgetRepository)
        {
            _repo=budgetRepository;
        }

        public async Task<CfgVm> GetCfg() { return await _repo.GetCfgAsync(); }
        public async Task<IEnumerable<UserLockedItemVm>> GetAllUserLockedItems() { return await _repo.GetAllUserLockedItemsAsync(); }
        public async Task<int> ExecuteRawSql(string sql, object parameters = null) { return await _repo.ExecuteRawSqlAsync(sql,parameters); }
        public async Task<int> ExecuteRawScalarSql(string sql, object parameters = null) { return await _repo.ExecuteRawScalarSqlAsync(sql, parameters); }
        public async Task<IEnumerable<T>> QueryRawList<T>(string sql, object parameters = null) { return await _repo.QueryRawListAsync<T>(sql, parameters); }
        public async Task<T> QueryRaw<T>(string sql, object parameters = null) { return await _repo.QueryRawAsync<T>(sql, parameters); }

    }
}
