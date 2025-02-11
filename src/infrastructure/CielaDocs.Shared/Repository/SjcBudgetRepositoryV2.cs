using CielaDocs.Application.Models;
using CielaDocs.Shared.DataAccess;

using Dapper;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Shared.Repository
{
    public class SjcBudgetRepositoryV2:ISjcBudgetRepositoryV2
    {
        private SjcBudgetContext _context;

        public SjcBudgetRepositoryV2(SjcBudgetContext context)
        {
            this._context = context;
        }

        public async Task<BudgetPeriodVm> GetActiveBudgetPeriodAsync()
        {
            string sql = $@"SELECT [Id]
                  ,[Y1]
                  ,[Y2]
                  ,[Y3]
                  ,[Y4]
                  ,[IsActive]
                  ,[IsUsable]
                  ,[ActiveFrom]
                  ,[ActiveTo]
                  ,[Note]
              FROM BudgetPeriod where IsActive=1";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QuerySingleOrDefaultAsync<BudgetPeriodVm>(sql);
            return result;
        }

        public async Task<IEnumerable<BudgetPeriodVm>> GetInActiveBudgetPeriodsAsync()
        {
            string sql = $@"SELECT [Id]
                  ,[Y1]
                  ,[Y2]
                  ,[Y3]
                  ,[Y4]
                  ,[IsActive]
                  ,[IsUsable]
                  ,[ActiveFrom]
                  ,[ActiveTo]
                  ,[Note]
              FROM BudgetPeriod where IsActive=0";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<BudgetPeriodVm>(sql);
            return result;
        }
    }
}
