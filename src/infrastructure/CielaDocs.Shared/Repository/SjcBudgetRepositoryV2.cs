using CielaDocs.Application.Models;
using CielaDocs.Application.Utils;
using CielaDocs.Domain.Entities;
using CielaDocs.Domain.Entities.v2;
using CielaDocs.Shared.DataAccess;

using Dapper;

using DocumentFormat.OpenXml.Office2010.Excel;

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
        public async Task<int> GetCurrentYearAsync() {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QuerySingleOrDefaultAsync<int?>("Select CurrentYear from Cfg");
            return result??0;
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
        public async Task<int?> SpEndCurrentYearDataAsync()
        {

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            var ret = await connection.ExecuteAsync("sp_EndCurrentYear", parameters, commandType: CommandType.StoredProcedure);
            return ret;
        }
        public async Task<int?> SpEndCurrentAppMonthDataAsync()
        {

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            var ret = await connection.ExecuteAsync("sp_EndCurrentAppMonth", parameters, commandType: CommandType.StoredProcedure);
            return ret;
        }
        public async Task<IEnumerable<MetricsFieldInProgramVm>> GetMetricsFieldInProgramByMainIndicatorIdAsync(int? id)
        {
            string sql = $@"SELECT [Id]
              ,[MainIndicatorsId]
              ,[FunctionalSubAreaId]
              ,[Code]
              ,[Name]
              ,[NeededFor]
              ,[IsActive]
              ,[TypeOfIndicatorId]
          FROM MetricsFieldInProgram where MainIndicatorsId={id}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<MetricsFieldInProgramVm>(sql);
            return result?.ToList();
        }
        private async Task<IEnumerable<MetricsFieldInProgramVm>> GetMetricsFieldInProgramByMainIndicatorsId(int id) {
            string sql = $@"SELECT [Id]
              ,[MainIndicatorsId]
              ,[FunctionalSubAreaId]
              ,[Code]
              ,[Name]
              ,[NeededFor]
              ,[IsActive]
              ,[TypeOfIndicatorId]
          FROM  MetricsFieldInProgram where MainIndicatorsId={id}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<MetricsFieldInProgramVm>(sql);
            return result?.ToList();
        }
        public async Task<IEnumerable<MetricsFieldInProgramItemVm>> CreateMetricsFieldInProgramItemExists(MainData md) {
            string sql = $@"SELECT [Id]
                      ,[MetricsFieldInProgramId]
                      ,[MainIndicatorsId]
                      ,[FunctionalSubAreaId]
                      ,[CourtId]
                      ,[NMonth]
                      ,[NYear]
                      ,[Nvalue]
                      ,[Datum]
                      ,[EnteredOn] FROM MetricsFieldInProgramItem where MainIndicatorsId={md?.MainIndicatorsId??0} and FunctionalSubAreaId={md?.FunctionalSubAreaId??0} and CourtId={md?.CourtId??0} and NYear={md?.Nyear} and NMonth={md?.Nmonth} ";
            
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<MetricsFieldInProgramItemVm>(sql);
            if (!result.Any()) {
                var metrics = await GetMetricsFieldInProgramByMainIndicatorsId(md?.MainIndicatorsId??0);
                if (metrics.Any()) {
                    string sqlins = string.Empty;
                    foreach (var item in metrics) {
                        sqlins = $@"INSERT INTO [dbo].[MetricsFieldInProgramItem]
                               ([MetricsFieldInProgramId]
                               ,[MainIndicatorsId]
                               ,[FunctionalSubAreaId]
                               ,[CourtId]
                               ,[NMonth]
                               ,[NYear]
                               ,[Nvalue]
                               )
                         VALUES
                               ({item?.Id ?? 0}
                               ,{item?.MainIndicatorsId ?? 0}
                               ,{item?.FunctionalSubAreaId ?? 0}
                               ,{md?.CourtId ?? 0}
                               ,{md?.Nmonth ?? 0}
                               ,{md?.Nyear ?? 0}
                               ,{0}
                               )";
                       
                        var affectedRows = await connection.ExecuteAsync(sqlins);
                    }
                    return await connection.QueryAsync<MetricsFieldInProgramItemVm>(sql);
                }
            }
            return result;
        }
        public async Task<IEnumerable<MetricsFieldInProgramItemVm>> GetMetricsFieldInProgramItemByMainIndicatorsId(int id,int? courtId, int? nm, int? ny) {
            string sql = $@"SELECT a.Id
                  ,a.MetricsFieldInProgramId
                  ,a.MainIndicatorsId
                  ,a.FunctionalSubAreaId
                  ,a.CourtId
                  ,a.NMonth
                  ,a.NYear
                  ,a.Nvalue
                  ,a.Datum
                  ,a.EnteredOn
                  ,m.Code
                  ,m.Name
                  FROM MetricsFieldInProgramItem a
                  left join MetricsFieldInProgram m on a.MetricsFieldInProgramId=m.id
                  where a.MainIndicatorsId={id} and a.CourtId={courtId??0} and NMonth={nm??0} and NYear={ny??0}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<MetricsFieldInProgramItemVm>(sql);
            return result?.ToList();
        }
        public async Task<decimal?> SumMetricsFieldInProgramItemByMainIndicatorsId(int id, int? nm1, int? nm2, int? ny)
        {
            string sql = $@"SELECT coalesce(sum(a.Nvalue),0)
                  
                  FROM MetricsFieldInProgramItem a
                  left join MetricsFieldInProgram m on a.MetricsFieldInProgramId=m.id
                  where a.MainIndicatorsId={id} and a.NMonth>={nm1} and NMonth<={nm2 ?? 0} and NYear={ny ?? 0}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QuerySingleOrDefaultAsync<decimal?>(sql);
            return result??0;
        }
        public async Task<MainIndicatorsVm> GetMainIndicatorsById(int Id)
        {
            string sql = $@"select a.Id,a.FunctionalSubAreaId,a.Code,a.Name,a.MeasureId,a.IsActive,a.Calculation,a.Gkey,f.Name as FunctionalSubAreaName, c.Name as MeasureName,t.Name as TypeOfIndicatorName
                            from MainIndicators a
                            join FunctionalSubArea f on a.FunctionalSubAreaId=f.Id
                            join Measure c on a.MeasureId=c.Id
                            join TypeOfIndicator t on a.TypeOfIndicatorId=t.id
                            where a.Id=@Id";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QuerySingleOrDefaultAsync<MainIndicatorsVm>(sql, new { Id = Id });
            return result;
        }


        //-----------------indicators
        public async Task<IEnumerable<IndicatorDataHVm>> GetIndicatorDataForEndingPeriod(int id) {
            var activeperiod = await GetActiveBudgetPeriodByIdAsync(id);
            string sql2 = @"select a.Id, a.FunctionalSubAreaId,a.Code,a.Name,a.MeasureId,a.TypeOfIndicatorID,a.Calculation,t1.PlannedYear as PlannedYear1,t2.PlannedYear as PlannedYear2,t3.PlannedYear as PlannedYear3,t4.PlannedYear as PlannedYear4 ,t1.Nvalue1, t2.Nvalue2,t3.Nvalue3,t4.Nvalue4 
     from MainIndicators a 
     left join    (
                      select  MainIndicatorId,PlannedYear, COALESCE(NValue,0) as Nvalue1
                      from    IndicatorData
                      group by
                              MainIndicatorId,PlannedYear,Nvalue
                      ) t1
              on      t1.MainIndicatorId=a.Id and t1.PlannedYear=@NY

	  left join    (
                      select  MainIndicatorId,PlannedYear, COALESCE(NValue,0)  as Nvalue2
                      from    IndicatorData
                      group by
                              MainIndicatorId,PlannedYear,Nvalue
                      ) t2
              on      t2.MainIndicatorId=a.id and  t2.PlannedYear=@NY1
	  left join    (
                      select  MainIndicatorId,PlannedYear,COALESCE(NValue,0) as Nvalue3
                      from    IndicatorData
                      group by
                              MainIndicatorId,PlannedYear,Nvalue
                      ) t3
              on      t3.MainIndicatorId=a.Id and  t3.PlannedYear=@NY2
    left join    (
                      select  MainIndicatorId,PlannedYear, COALESCE(NValue,0) as Nvalue4
                      from    IndicatorData
                      group by
                              MainIndicatorId,PlannedYear,Nvalue
                      ) t4
              on      t4.MainIndicatorId=a.id and  t4.PlannedYear=@NY3";
            var parameters = new
            {


                NY = activeperiod?.Y1 ?? 0,
                NY1 = activeperiod?.Y2 ?? 0,
                NY2 = activeperiod?.Y3 ?? 0,
                NY3 = activeperiod?.Y4 ?? 0
            };
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IndicatorDataHVm>(sql2, parameters);
            return result?.ToList();
        }
        public async Task<bool> GetIndicatorDataHExistsAsync(int? budgetPeriodId, int? functionalSubAreaId, int? mainIndicatorId, int? plannedYear1)
        {
            string sql = $@"SELECT top 1 [Id] FROM IndicatorDataH where BudgetPeriodId={budgetPeriodId ?? 0} and MainIndicatorId={mainIndicatorId ?? 0}  and FunctionalSubAreaId={functionalSubAreaId ?? 0} and PlannedYear1={plannedYear1 ?? 0}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QuerySingleOrDefaultAsync<int?>(sql);
            return (result != null);
        }
        public async Task<int> InsertIntoIndicatorDataHAsync(IndicatorDataHVm data, int periodId)
        {
            var sql = $@"INSERT INTO [dbo].[IndicatorDataH]
           ([BudgetPeriodId]
           ,[MainIndicatorId]
           ,[FunctionalSubAreaId]
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
           ,{data?.Id ?? 0}
           ,{data?.FunctionalSubAreaId ?? 0}
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
        public async Task<bool> GetIndicatorDataCourtHExistsAsync(int? budgetPeriodId, int? courtId, int? functionalSubAreaId, int? mainIndicatorId, int? plannedYear1)
        {
            string sql = $@"SELECT top 1 [Id] FROM IndicatorDataCourtH where BudgetPeriodId={budgetPeriodId ?? 0} and MainIndicatorId={mainIndicatorId ?? 0} and CourtId={courtId ?? 0} and FunctionalSubAreaId={functionalSubAreaId ?? 0}  and PlannedYear1={plannedYear1}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QuerySingleOrDefaultAsync<int?>(sql);
            return (result != null);
        }
        private async Task<IEnumerable<IndicatorDataCourtHVm>> GetIndicatorDataCourtForEndingPeriodByCourtId(int id, int courtId)
        {
            var activeperiod = await GetActiveBudgetPeriodByIdAsync(id);

           string sql2 = @"select a.Id, a.FunctionalSubAreaId,t1.CourtId,a.Code,a.Name,a.MeasureId,a.TypeOfIndicatorId,a.Calculation,t1.PlannedYear as PlannedYear1,t2.PlannedYear as PlannedYear2,t3.PlannedYear as PlannedYear3,t4.PlannedYear as PlannedYear4 ,t1.Nvalue1, t2.Nvalue2,t3.Nvalue3,t4.Nvalue4 
     from MainIndicators a 
     left join    (
                      select  MainIndicatorId,CourtId,PlannedYear, COALESCE(NValue,0) as Nvalue1
                      from    IndicatorDataCourt
                      group by
                              MainIndicatorId,CourtId,PlannedYear,Nvalue
                      ) t1
              on      t1.MainIndicatorId=a.Id and t1.PlannedYear=@NY

	  left join    (
                      select  MainIndicatorId,CourtId,PlannedYear, COALESCE(NValue,0)  as Nvalue2
                      from    IndicatorDataCourt
                      group by
                              MainIndicatorId,CourtId,PlannedYear,Nvalue
                      ) t2
              on      t2.MainIndicatorId=a.id and  t2.PlannedYear=@NY1
	  left join    (
                      select  MainIndicatorId,CourtId,PlannedYear,COALESCE(NValue,0) as Nvalue3
                      from    IndicatorDataCourt
                      group by
                              MainIndicatorId,CourtId,PlannedYear,Nvalue
                      ) t3
              on      t3.MainIndicatorId=a.Id and  t3.PlannedYear=@NY2
    left join    (
                      select  MainIndicatorId,CourtId,PlannedYear, COALESCE(NValue,0) as Nvalue4
                      from    IndicatorDataCourt
                      group by
                              MainIndicatorId,CourtId,PlannedYear,Nvalue
                      ) t4
              on      t4.MainIndicatorId=a.id and  t4.PlannedYear=@NY3

            where t1.courtId = @CourtId and t2.courtId = @CourtId and t3.courtId = @CourtId and t4.courtId = @CourtId";
            


            var parameters = new
            {

                CourtId = courtId,
                NY = activeperiod?.Y1 ?? 0,
                NY1 = activeperiod?.Y2 ?? 0,
                NY2 = activeperiod?.Y3 ?? 0,
                NY3 = activeperiod?.Y4 ?? 0
            };
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IndicatorDataCourtHVm>(sql2, parameters);
            return result?.ToList();
        }
        public async Task<IEnumerable<IndicatorDataCourtHVm>> GetIndicatorDataCourtForEndingPeriod(int id)
        {
            List<IndicatorDataCourtHVm> result = new();
            var courts = await GetCourtsAsync();
            if (courts.Any())
            {
                foreach (var item in courts)
                {
                    result.AddRange(await GetIndicatorDataCourtForEndingPeriodByCourtId(id, item?.Id ?? 0));
                }
            }
            return result;
        }
        public async Task<int> InsertIntoIndicatorDataCourtHAsync(IndicatorDataCourtHVm data, int periodId)
        {
            var sql = $@"INSERT INTO [dbo].[IndicatorDataCourtH]
           ([BudgetPeriodId]
           ,[MainIndicatorId]
           ,[FunctionalSubAreaId]
           ,[CourtId]
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
           ,{data?.Id ?? 0}
           ,{data?.FunctionalSubAreaId ?? 0}
           ,{data?.CourtId ?? 0}
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
        public async Task<int> UpdateIndicatorData3YValueByIdAsync(int? id, string fieldName, decimal? val, int? nYear)
        {
            var indicatorReq = await GetIndicatorDataById(id ?? 0);
            var rec = await GetMainIndicatorsById(indicatorReq?.MainIndicatorId ?? 0);
            int currentYear = nYear ?? 0;

            switch (fieldName.ToLower())
            {
                case "nval2": currentYear = currentYear + 1; break;
                case "nval3": currentYear = currentYear + 2; break;
                case "nval4": currentYear = currentYear + 3; break;
            }
            var sql = $@"UPDATE IndicatorData SET Nvalue = {val}, EnteredDate=getDate() WHERE MainIndicatorId={rec?.Id} and FunctionalSubAreaId={rec?.FunctionalSubAreaId} and PlannedYear={currentYear} ";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = await connection.ExecuteAsync(sql);

            return affectedRows;
        }
        public async Task<int> UpdateIndicatorDataCourt3YValueByIdAsync(int? id, string fieldName, decimal? val)
        {
            var indicatorDataCourt = await GetIndicatorDataCourtById(id ?? 0);
            var rec = await GetMainIndicatorsById(indicatorDataCourt?.MainIndicatorId ?? 0);
            int currentYear = indicatorDataCourt?.PlannedYear??0;

            switch (fieldName.ToLower())
            {
                case "nval2":currentYear = currentYear + 1;  break;
                case "nval3": currentYear = currentYear + 2; break;
                case "nval4": currentYear = currentYear + 3; break;
            }
            var sql = $@"UPDATE IndicatorDataCourt SET Nvalue = {val}, EnteredDate=getDate() WHERE  MainIndicatorId={indicatorDataCourt?.MainIndicatorId ?? 0} and CourtId={indicatorDataCourt?.CourtId ?? 0} and FunctionalSubAreaId={rec?.FunctionalSubAreaId} and PlannedYear={currentYear}  ";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = await connection.ExecuteAsync(sql);

            return affectedRows;
        }
        public async Task<int> UpdateIndicatorData1YValueByIdAsync(int? id, string fieldName, decimal? val, int? nYear)
        {
            var indicatorReq = await GetIndicatorDataById(id ?? 0);
            var rec = await GetMainIndicatorsById(indicatorReq?.MainIndicatorId ?? 0);
            int currentYear = nYear ?? 0;

            var sql = $@"UPDATE IndicatorData SET Nvalue = {val}, EnteredDate=getDate() WHERE MainIndicatorId={rec?.Id} and FunctionalSubAreaId={rec?.FunctionalSubAreaId} and PlannedYear={currentYear} ";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = await connection.ExecuteAsync(sql);

            return affectedRows;
        }
        public async Task<int> UpdateIndicatorDataCourt1YValueByIdAsync(int? id, string fieldName, decimal? val)
        {
            var indicatorDataCourt = await GetIndicatorDataCourtById(id ?? 0);
            var rec = await GetMainIndicatorsById(indicatorDataCourt?.MainIndicatorId ?? 0);
            int currentYear = indicatorDataCourt?.PlannedYear ?? 0;

            var sql = $@"UPDATE IndicatorDataCourt SET Nvalue = {val}, EnteredDate=getDate() WHERE  MainIndicatorId={indicatorDataCourt?.MainIndicatorId ?? 0} and CourtId={indicatorDataCourt?.CourtId ?? 0} and FunctionalSubAreaId={rec?.FunctionalSubAreaId} and PlannedYear={currentYear}  ";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = await connection.ExecuteAsync(sql);

            return affectedRows;
        }
        public async Task<IEnumerable<IndicatorDataCourt3Y>> GetIndicatorDataCourt3YAsync(int functionalSubAreaId, int ny, int? mainIndicatorId) 
        {


            string sql2 = $@"SELECT  a.Id
                      ,a.MainIndicatorId
                      ,a.CourtId
	                  ,a.functionalSubAreaId
                      ,m.Code
                      ,m.Name
	                  ,m.MeasureId
	                  ,m.IsActive
	                  ,m.TypeOfIndicatorId
	                  ,m.Calculation
	                  ,z.Name as MeasureName
	                  ,t.NAme as TypeOfIndicatorName
	                  ,a.PlannedYear
	                  ,a.Nvalue as nval1
	                  ,b.nvalue as nval2
	                  ,c.Nvalue as nval3
	                  ,d.Nvalue as nval4
	                  ,r.Name as CourtName
     
                  FROM IndicatorDataCourt a
                  left join MainIndicators m on  a.MainIndicatorId=m.id
                  left join Measure z on m.MeasureId=z.id
                  left join TypeOfIndicator t on m.TypeOfIndicatorId=t.id
                  left join court r on a.CourtId=r.id
                  left join IndicatorDataCourt b on a.FunctionalSubAreaId=b.FunctionalSubAreaId and a.MainIndicatorId=b.MainIndicatorId and a.CourtId=b.CourtId  and b.PlannedYear=a.PlannedYear+1
                  left join IndicatorDataCourt c on a.FunctionalSubAreaId=c.FunctionalSubAreaId and a.MainIndicatorId=c.MainIndicatorId and a.CourtId=c.CourtId and  c.PlannedYear=a.PlannedYear+2
                  left join IndicatorDataCourt d on a.FunctionalSubAreaId=d.FunctionalSubAreaId and a.MainIndicatorId=d.MainIndicatorId and a.CourtId=d.CourtId  and d.PlannedYear=a.PlannedYear+3

	              where a.MainIndicatorId={mainIndicatorId??0} and a.FunctionalSubAreaId={functionalSubAreaId} and a.PlannedYear={ny} ";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IndicatorDataCourt3Y>(sql2);
            return result?.ToList();
        }
        public async Task<IEnumerable<IndicatorDataCourt1Y>> GetIndicatorDataCourt1YAsync(int functionalSubAreaId, int ny, int? mainIndicatorId)
        {


            string sql2 = $@"SELECT  a.Id
                      ,a.MainIndicatorId
                      ,a.CourtId
	                  ,a.functionalSubAreaId
                      ,m.Code
                      ,m.Name
	                  ,m.MeasureId
	                  ,m.IsActive
	                  ,m.TypeOfIndicatorId
	                  ,m.Calculation
	                  ,z.Name as MeasureName
	                  ,t.NAme as TypeOfIndicatorName
                      ,a.Nvalue 
                      ,a.EnteredDate
	                  ,a.PlannedYear
	                  ,a.ApprovedValue
                      ,a.CalculatedValue
	                  ,r.Name as CourtName
     
                  FROM IndicatorDataCourt a
                  left join MainIndicators m on  a.MainIndicatorId=m.id
                  left join Measure z on m.MeasureId=z.id
                  left join TypeOfIndicator t on m.TypeOfIndicatorId=t.id
                  left join court r on a.CourtId=r.id
                 

	              where a.MainIndicatorId={mainIndicatorId ?? 0} and a.FunctionalSubAreaId={functionalSubAreaId} and a.PlannedYear={ny} ";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IndicatorDataCourt1Y>(sql2);
            return result?.ToList();
        }
        public async Task<IEnumerable<IndicatorDataCourt3Y>> GetIndicatorDataCourt3YByCourtIdAsync(int? functionalSubAreaId, int? ny, int? courtId)
        {
            string sql2 = $@"SELECT  a.Id
                      ,a.MainIndicatorId
                      ,a.CourtId
	                  ,a.functionalSubAreaId
                      ,m.Code
                      ,m.Name
	                  ,m.MeasureId
	                  ,m.IsActive
	                  ,m.TypeOfIndicatorId
	                  ,m.Calculation
	                  ,z.Name as MeasureName
	                  ,t.NAme as TypeOfIndicatorName
	                  ,a.PlannedYear
	                  ,a.Nvalue as nval1
	                  ,b.nvalue as nval2
	                  ,c.Nvalue as nval3
	                  ,d.Nvalue as nval4
	                  ,r.Name as CourtName
     
                  FROM IndicatorDataCourt a
                  left join MainIndicators m on  a.MainIndicatorId=m.id
                  left join Measure z on m.MeasureId=z.id
                  left join TypeOfIndicator t on m.TypeOfIndicatorId=t.id
                  left join court r on a.CourtId=r.id
                  left join IndicatorDataCourt b on a.FunctionalSubAreaId=b.FunctionalSubAreaId and a.MainIndicatorId=b.MainIndicatorId and a.CourtId=b.CourtId  and b.PlannedYear=a.PlannedYear+1
                  left join IndicatorDataCourt c on a.FunctionalSubAreaId=c.FunctionalSubAreaId and a.MainIndicatorId=c.MainIndicatorId and a.CourtId=c.CourtId and  c.PlannedYear=a.PlannedYear+2
                  left join IndicatorDataCourt d on a.FunctionalSubAreaId=d.FunctionalSubAreaId and a.MainIndicatorId=d.MainIndicatorId and a.CourtId=d.CourtId  and d.PlannedYear=a.PlannedYear+3

	              where a.CourtId={courtId ?? 0} and a.FunctionalSubAreaId={functionalSubAreaId} and a.PlannedYear={ny} ";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IndicatorDataCourt3Y>(sql2);
            return result?.ToList();
        }
        public async Task<IEnumerable<IndicatorDataCourt1Y>> GetIndicatorDataCourt1YByCourtIdAsync(int? functionalSubAreaId, int? ny, int? courtId)
        {
            string sql2 = $@"SELECT  a.Id
                      ,a.MainIndicatorId
                      ,a.CourtId
	                  ,a.functionalSubAreaId
                      ,m.Code
                      ,m.Name
	                  ,m.MeasureId
	                  ,m.IsActive
	                  ,m.TypeOfIndicatorId
	                  ,m.Calculation
	                  ,z.Name as MeasureName
	                  ,t.NAme as TypeOfIndicatorName
                      ,a.Nvalue 
                      ,a.EnteredDate
	                  ,a.PlannedYear
	                  ,a.ApprovedValue
                      ,a.CalculatedValue
	                  ,r.Name as CourtName
     
                  FROM IndicatorDataCourt a
                  left join MainIndicators m on  a.MainIndicatorId=m.id
                  left join Measure z on m.MeasureId=z.id
                  left join TypeOfIndicator t on m.TypeOfIndicatorId=t.id
                  left join court r on a.CourtId=r.id
                 

	              where a.CourtId={courtId ?? 0} and a.FunctionalSubAreaId={functionalSubAreaId} and a.PlannedYear={ny} ";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IndicatorDataCourt1Y>(sql2);
            return result?.ToList();
        }
        public async Task<IndicatorDataVm> GetIndicatorDataById(int Id) {
           
            string sql = $@"SELECT [Id],[MainIndicatorId] ,[FunctionalSubAreaId],[Nvalue],[EnteredDate],[PlannedYear],[ApprovedValue],[CalculatedValue],[BudgetPeriodId]  FROM [dbo].[IndicatorData] where id={Id}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QuerySingleOrDefaultAsync<IndicatorDataVm>(sql);
            return result;
        }
        public async Task<IndicatorDataCourtVm> GetIndicatorDataCourtById(int Id)
        {

            string sql = $@"SELECT [Id],[MainIndicatorId] ,[FunctionalSubAreaId],CourtId,[Nvalue],[EnteredDate],[PlannedYear],[ApprovedValue],[CalculatedValue],[BudgetPeriodId]  FROM [dbo].[IndicatorDataCourt] where id={Id}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QuerySingleOrDefaultAsync<IndicatorDataCourtVm>(sql);
            return result;
        }
    }
}
