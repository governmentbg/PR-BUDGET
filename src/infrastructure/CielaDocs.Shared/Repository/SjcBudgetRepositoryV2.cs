using CielaDocs.Application.Models;
using CielaDocs.Application.Utils;
using CielaDocs.Shared.DataAccess;

using Dapper;

using MimeKit.Utils;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
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
        public async Task<BudgetPeriodVm> GetActiveBudgetPeriodByIdAsync(int id) {
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
              FROM BudgetPeriod where id={id}";
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
        public async Task<IEnumerable<BudgetPeriodVm>> GetBudgetPeriodsAsync()
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
              FROM BudgetPeriod order by id";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<BudgetPeriodVm>(sql);
            return result;
        }
        public async Task<IEnumerable<ProgramDataHVm>> GetProgramDataForEndingPeriod(int id) {

            var activeperiod = await GetActiveBudgetPeriodByIdAsync(id);
            string sql2 = @"select a.Id,a.FunctionalAreaId,a.FunctionalSubAreaId,a.FunctionalActionId,a.RowNum,a.PrnCode,a.Name,a.ParentRowNum,a.CurrencyId,a.CurrencyMeasureId,
t1.PlannedYear as PlannedYear1,t2.PlannedYear as PlannedYear2,t3.PlannedYear as PlannedYear3,t4.PlannedYear as PlannedYear4 ,t1.Nvalue1, t2.Nvalue2,t3.Nvalue3,t4.Nvalue4 
     from ProgramDef a 
     left join    (
                      select  FunctionalSubAreaId,RowNum,PlannedYear, COALESCE(NValue,0) as Nvalue1
                      from    ProgramData
                      group by
                              FunctionalSubAreaId,RowNum,PlannedYear,Nvalue
                      ) t1
              on      t1.FunctionalSubAreaId=a.FunctionalSubAreaId and t1.RowNum=a.RowNum and t1.PlannedYear=@NY

	  left join    (
                      select  FunctionalSubAreaId,RowNum,PlannedYear, COALESCE(NValue,0)  as Nvalue2
                      from    ProgramData
                      group by
                              FunctionalSubAreaId,RowNum,PlannedYear,Nvalue
                      ) t2
              on      t2.FunctionalSubAreaId=a.FunctionalSubAreaId and t2.RowNum=a.RowNum and t2.PlannedYear=@NY1
	  left join    (
                      select  FunctionalSubAreaId,RowNum,PlannedYear,COALESCE(NValue,0) as Nvalue3
                      from    ProgramData
                      group by
                              FunctionalSubAreaId,RowNum,PlannedYear,Nvalue
                      ) t3
              on      t3.FunctionalSubAreaId=a.FunctionalSubAreaId and t3.RowNum=a.RowNum and t3.PlannedYear=@NY2
    left join    (
                      select  FunctionalSubAreaId,RowNum,PlannedYear, COALESCE(NValue,0) as Nvalue4
                      from    ProgramData
                      group by
                              FunctionalSubAreaId,RowNum,PlannedYear,Nvalue
                      ) t4
              on      t4.FunctionalSubAreaId=a.FunctionalSubAreaId and t4.RowNum=a.RowNum and t4.PlannedYear=@NY3";
            var parameters = new
            {
               

                NY = activeperiod?.Y1??0,
                NY1 = activeperiod?.Y2 ?? 0,
                NY2 = activeperiod?.Y3 ?? 0,
                NY3 = activeperiod?.Y4 ?? 0
            };
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDataHVm>(sql2, parameters);
            return result?.ToList();
        }
        private async Task<IEnumerable<ProgramDataCourtHVm>> GetProgramDataCourtForEndingPeriodByCourtId(int id, int courtId) {
            var activeperiod = await GetActiveBudgetPeriodByIdAsync(id);





            string sql2 = @"select a.Id,t1.CourtId,a.FunctionalAreaId,a.FunctionalSubAreaId,a.FunctionalActionId,a.RowNum,a.PrnCode,a.Name,a.ParentRowNum,a.CurrencyId,a.CurrencyMeasureId,
t1.PlannedYear as PlannedYear1,t2.PlannedYear as PlannedYear2,t3.PlannedYear as PlannedYear3,t4.PlannedYear as PlannedYear4 ,t1.Nvalue1, t2.Nvalue2,t3.Nvalue3,t4.Nvalue4 
     from ProgramDef a 
     left join    (
                      select  CourtId,FunctionalSubAreaId,RowNum,PlannedYear, COALESCE(NValue,0) as Nvalue1
                      from    ProgramDataCourt
                      group by
                              CourtId,FunctionalSubAreaId,RowNum,PlannedYear,Nvalue
                      ) t1
              on      t1.FunctionalSubAreaId=a.FunctionalSubAreaId and t1.RowNum=a.RowNum and t1.PlannedYear=@Ny 

	  left join    (
                      select  CourtId,FunctionalSubAreaId,RowNum,PlannedYear, COALESCE(NValue,0)  as Nvalue2
                      from    ProgramDataCourt
                      group by
                              CourtId,FunctionalSubAreaId,RowNum,PlannedYear,Nvalue
                      ) t2
              on      t2.FunctionalSubAreaId=a.FunctionalSubAreaId and t2.RowNum=a.RowNum and t2.PlannedYear=@Ny1 
	  left join    (
                      select  CourtId,FunctionalSubAreaId,RowNum,PlannedYear,COALESCE(NValue,0) as Nvalue3
                      from    ProgramDataCourt
                      group by
                              CourtId,FunctionalSubAreaId,RowNum,PlannedYear,Nvalue
                      ) t3
              on      t3.FunctionalSubAreaId=a.FunctionalSubAreaId and t3.RowNum=a.RowNum and t3.PlannedYear=@Ny2 
    left join    (
                      select  CourtId,FunctionalSubAreaId,RowNum,PlannedYear, COALESCE(NValue,0) as Nvalue4
                      from    ProgramDataCourt
                      group by
                              CourtId,FunctionalSubAreaId,RowNum,PlannedYear,Nvalue
                      ) t4
              on      t4.FunctionalSubAreaId=a.FunctionalSubAreaId and t4.RowNum=a.RowNum and t4.PlannedYear=@Ny3 
            where t1.courtId=@CourtId and t2.courtId=@CourtId and t3.courtId=@CourtId and t4.courtId=@CourtId";
            var parameters = new
            {

                CourtId=courtId,
                NY = activeperiod?.Y1 ?? 0,
                NY1 = activeperiod?.Y2 ?? 0,
                NY2 = activeperiod?.Y3 ?? 0,
                NY3 = activeperiod?.Y4 ?? 0
            };
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDataCourtHVm>(sql2, parameters);
            return result?.ToList();
        }
        public async Task<IEnumerable<CourtsVm>> GetCourtsAsync()
        {
            string sql = $@"select c.Id,c.Num,c.CourtTypeId,c.Name,c.IsActive,c.CourtGuid ,t.Name as CourtTypeName,i.Name as InstitutionTypeName
                            from Court c
                            join CourtType t on c.CourtTypeId=t.Id
                            join InstitutionType i on t.InstitutionTypeId=i.Id";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<CourtsVm>(sql);
            return result?.ToList();
        }
        public async Task<IEnumerable<ProgramDataCourtHVm>> GetProgramDataCourtForEndingPeriod(int id)
        {
            List<ProgramDataCourtHVm> result= new();
            var courts = await GetCourtsAsync();
            if (courts.Any()) {
                foreach (var item in courts) {
                    result.AddRange(await GetProgramDataCourtForEndingPeriodByCourtId(id, item?.Id ?? 0));
                }
            }
            return result;
        }
        public async Task<bool> GetProgramDataHExistsAsync(int? budgetPeriodId, int? functionalSubAreaId, int? rowNum, int? plannedYear1) {
            string sql = $@"SELECT top 1 [Id] FROM ProgramDataH where BudgetPeriodId={budgetPeriodId??0} and FunctionalSubAreaId={functionalSubAreaId??0} and RowNum={rowNum??0} and PlannedYear1={plannedYear1??0}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QuerySingleOrDefaultAsync<int?>(sql);
            return (result!=null);
        }
        public async Task<int> InsertIntoProgramDataHAsync(ProgramDataHVm data, int periodId) {
            var sql = $@"INSERT INTO [dbo].[ProgramDataH]
           ([BudgetPeriodId]
           ,[ProgramDefNum]
           ,[FunctionalAreaId]
           ,[FunctionalSubAreaId]
           ,[FunctionalActionId]
           ,[RowNum]
           ,[RowCode]
           ,[PrnCode]
           ,[Name]
           ,[ParentRowNum]
           ,[CurrencyId]
           ,[CurrencyMeasureId]
           ,[Datum]
           ,[PlannedYear1]
           ,[PlannedYear2]
           ,[PlannedYear3]
           ,[PlannedYear4]
           ,[PlannedYear5]
           ,[Nvalue1]
           ,[Nvalue2]
           ,[Nvalue3]
           ,[Nvalue4]
           ,[Nvalue5])
     VALUES
           ({periodId}
           ,{data?.ProgramDefNum??0}
           ,{data?.FunctionalAreaId??0}
           ,{data?.FunctionalSubAreaId??0}
           ,{data?.FunctionalActionId??0}
           ,{data?.RowNum??0}
           ,'{data?.RowCode}'
           ,'{data?.PrnCode}'
           ,'{data?.Name}'
           ,{data?.ParentRowNum??0}
           ,{data?.CurrencyId??0}
           ,{data?.CurrencyMeasureId??0}
           ,'{Utils.GetSqlDateTime(DateTime.Now,0)}'
           ,{data?.PlannedYear1??0}
           ,{data?.PlannedYear2??0}
           ,{data?.PlannedYear3 ?? 0}
           ,{data?.PlannedYear4 ?? 0}
           ,{0}
           ,{data?.Nvalue1??0}
           ,{data?.Nvalue2 ?? 0}
           ,{data?.Nvalue3 ?? 0}
           ,{data?.Nvalue4 ?? 0}
           ,{0})";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = await connection.ExecuteAsync(sql);

            return affectedRows;

        }
        public async Task<bool> GetProgramDataCourtHExistsAsync(int? budgetPeriodId, int? courtId, int? functionalSubAreaId, int? rowNum, int? plannedYear1)
        {
            string sql = $@"SELECT top 1 [Id] FROM ProgramDataCourtH where BudgetPeriodId={budgetPeriodId ?? 0} and CourtId={courtId??0} and FunctionalSubAreaId={functionalSubAreaId ?? 0} and RowNum={rowNum ?? 0} and PlannedYear1={plannedYear1}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QuerySingleOrDefaultAsync<int?>(sql);
            return (result != null);
        }
        public async Task<int> InsertIntoProgramDataCourtHAsync(ProgramDataCourtHVm data, int periodId)
        {
            var sql = $@"INSERT INTO [dbo].[ProgramDataCourtH]
           ([BudgetPeriodId]
            ,CourtId
           ,[ProgramDefNum]
           ,[FunctionalAreaId]
           ,[FunctionalSubAreaId]
           ,[FunctionalActionId]
           ,[RowNum]
           ,[RowCode]
           ,[PrnCode]
           ,[Name]
           ,[ParentRowNum]
           ,[CurrencyId]
           ,[CurrencyMeasureId]
           ,[Datum]
           ,[PlannedYear1]
           ,[PlannedYear2]
           ,[PlannedYear3]
           ,[PlannedYear4]
           ,[PlannedYear5]
           ,[Nvalue1]
           ,[Nvalue2]
           ,[Nvalue3]
           ,[Nvalue4]
           ,[Nvalue5])
     VALUES
           ({periodId}
           ,{data?.CourtId ?? 0}
           ,{data?.ProgramDefNum ?? 0}
           ,{data?.FunctionalAreaId ?? 0}
           ,{data?.FunctionalSubAreaId ?? 0}
           ,{data?.FunctionalActionId ?? 0}
           ,{data?.RowNum ?? 0}
           ,'{data?.RowCode}'
           ,'{data?.PrnCode}'
           ,'{data?.Name}'
           ,{data?.ParentRowNum ?? 0}
           ,{data?.CurrencyId ?? 0}
           ,{data?.CurrencyMeasureId ?? 0}
           ,'{Utils.GetSqlDateTime(DateTime.Now, 0)}'
           ,{data?.PlannedYear1 ?? 0}
           ,{data?.PlannedYear2 ?? 0}
           ,{data?.PlannedYear3 ?? 0}
           ,{data?.PlannedYear4 ?? 0}
           ,{0}
           ,{data?.Nvalue1 ?? 0}
           ,{data?.Nvalue2 ?? 0}
           ,{data?.Nvalue3 ?? 0}
           ,{data?.Nvalue4 ?? 0}
           ,{0})";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = await connection.ExecuteAsync(sql);

            return affectedRows;

        }

        public async Task<bool> GetProgramDataInstitutionHExistsAsync(int? budgetPeriodId, int? institutionTypeId, int? functionalSubAreaId, int? rowNum, int? plannedYear1)
        {
            string sql = $@"SELECT top 1 [Id] FROM ProgramDataInstitutionH where BudgetPeriodId={budgetPeriodId ?? 0} and InstitutionTypeId={institutionTypeId ?? 0} and FunctionalSubAreaId={functionalSubAreaId ?? 0} and RowNum={rowNum ?? 0} and PlannedYear1={plannedYear1}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QuerySingleOrDefaultAsync<int?>(sql);
            return (result != null);
        }
        private async Task<IEnumerable<IdNames>> GetInstitutionsAsync()
        {
            string sql = $@"select Id,Name from InstitutionType";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql);
            return result?.ToList();
        }
        private async Task<IEnumerable<ProgramDataInstitutionHVm>> GetProgramDataInstitutionForEndingPeriodByInstitutionTypeId(int id, int institutionTypeId)
        {
            var activeperiod = await GetActiveBudgetPeriodByIdAsync(id);





            string sql2 = @"select a.Id,t1.InstitutionTypeId,a.FunctionalAreaId,a.FunctionalSubAreaId,a.FunctionalActionId,a.RowNum,a.PrnCode,a.Name,a.ParentRowNum,a.CurrencyId,a.CurrencyMeasureId,
t1.PlannedYear as PlannedYear1,t2.PlannedYear as PlannedYear2,t3.PlannedYear as PlannedYear3,t4.PlannedYear as PlannedYear4 ,t1.Nvalue1, t2.Nvalue2,t3.Nvalue3,t4.Nvalue4 
     from ProgramDef a 
     left join    (
                      select  InstitutionTypeId,FunctionalSubAreaId,RowNum,PlannedYear, COALESCE(NValue,0) as Nvalue1
                      from    ProgramDataInstitution
                      group by
                              InstitutionTypeId,FunctionalSubAreaId,RowNum,PlannedYear,Nvalue
                      ) t1
              on      t1.FunctionalSubAreaId=a.FunctionalSubAreaId and t1.RowNum=a.RowNum and t1.PlannedYear=@Ny 

	  left join    (
                      select  InstitutionTypeId,FunctionalSubAreaId,RowNum,PlannedYear, COALESCE(NValue,0)  as Nvalue2
                      from    ProgramDataInstitution
                      group by
                              InstitutionTypeId,FunctionalSubAreaId,RowNum,PlannedYear,Nvalue
                      ) t2
              on      t2.FunctionalSubAreaId=a.FunctionalSubAreaId and t2.RowNum=a.RowNum and t2.PlannedYear=@Ny1 
	  left join    (
                      select  InstitutionTypeId,FunctionalSubAreaId,RowNum,PlannedYear,COALESCE(NValue,0) as Nvalue3
                      from    ProgramDataInstitution
                      group by
                              InstitutionTypeId,FunctionalSubAreaId,RowNum,PlannedYear,Nvalue
                      ) t3
              on      t3.FunctionalSubAreaId=a.FunctionalSubAreaId and t3.RowNum=a.RowNum and t3.PlannedYear=@Ny2 
    left join    (
                      select  InstitutionTypeId,FunctionalSubAreaId,RowNum,PlannedYear, COALESCE(NValue,0) as Nvalue4
                      from    ProgramDataInstitution
                      group by
                              InstitutionTypeId,FunctionalSubAreaId,RowNum,PlannedYear,Nvalue
                      ) t4
              on      t4.FunctionalSubAreaId=a.FunctionalSubAreaId and t4.RowNum=a.RowNum and t4.PlannedYear=@Ny3 
            where t1.InstitutionTypeId=@InstitutionTypeId and t2.InstitutionTypeId=@InstitutionTypeId and t3.InstitutionTypeId=@InstitutionTypeId and t4.InstitutionTypeId=@InstitutionTypeId";
            var parameters = new
            {

                InstitutionTypeId = institutionTypeId,
                NY = activeperiod?.Y1 ?? 0,
                NY1 = activeperiod?.Y2 ?? 0,
                NY2 = activeperiod?.Y3 ?? 0,
                NY3 = activeperiod?.Y4 ?? 0
            };
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDataInstitutionHVm>(sql2, parameters);
            return result?.ToList();
        }
        public async Task<IEnumerable<ProgramDataInstitutionHVm>> GetProgramDataInstitutionForEndingPeriod(int id)
        {
            List<ProgramDataInstitutionHVm> result = new();
            var institutions = await GetInstitutionsAsync();
            if (institutions.Any())
            {
                foreach (var item in institutions)
                {
                    result.AddRange(await GetProgramDataInstitutionForEndingPeriodByInstitutionTypeId(id, item?.Id ?? 0));
                }
            }
            return result;
        }
        public async Task<int> InsertIntoProgramDataInstitutionHAsync(ProgramDataInstitutionHVm data, int periodId)
        {
            var sql = $@"INSERT INTO [dbo].[ProgramDataInstitutionH]
           ([BudgetPeriodId]
            ,InstitutionTypeId
           ,[ProgramDefNum]
           ,[FunctionalAreaId]
           ,[FunctionalSubAreaId]
           ,[FunctionalActionId]
           ,[RowNum]
           ,[RowCode]
           ,[PrnCode]
           ,[Name]
           ,[ParentRowNum]
           ,[CurrencyId]
           ,[CurrencyMeasureId]
           ,[Datum]
           ,[PlannedYear1]
           ,[PlannedYear2]
           ,[PlannedYear3]
           ,[PlannedYear4]
           ,[PlannedYear5]
           ,[Nvalue1]
           ,[Nvalue2]
           ,[Nvalue3]
           ,[Nvalue4]
           ,[Nvalue5])
     VALUES
           ({periodId}
           ,{data?.InstitutionTypeId?? 0}
           ,{data?.ProgramDefNum ?? 0}
           ,{data?.FunctionalAreaId ?? 0}
           ,{data?.FunctionalSubAreaId ?? 0}
           ,{data?.FunctionalActionId ?? 0}
           ,{data?.RowNum ?? 0}
           ,'{data?.RowCode}'
           ,'{data?.PrnCode}'
           ,'{data?.Name}'
           ,{data?.ParentRowNum ?? 0}
           ,{data?.CurrencyId ?? 0}
           ,{data?.CurrencyMeasureId ?? 0}
           ,'{Utils.GetSqlDateTime(DateTime.Now, 0)}'
           ,{data?.PlannedYear1 ?? 0}
           ,{data?.PlannedYear2 ?? 0}
           ,{data?.PlannedYear3 ?? 0}
           ,{data?.PlannedYear4 ?? 0}
           ,{0}
           ,{data?.Nvalue1 ?? 0}
           ,{data?.Nvalue2 ?? 0}
           ,{data?.Nvalue3 ?? 0}
           ,{data?.Nvalue4 ?? 0}
           ,{0})";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = await connection.ExecuteAsync(sql);

            return affectedRows;
        }
        public async Task<int?> SpDeleteEndPeriodDataAsync(int budgetPeriodId)
        {

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("BudgetPeriodId", budgetPeriodId);
            var ret = await connection.ExecuteAsync("sp_DeleteEndPeriodData", parameters, commandType: CommandType.StoredProcedure);
            return ret;
        }
    }
}
