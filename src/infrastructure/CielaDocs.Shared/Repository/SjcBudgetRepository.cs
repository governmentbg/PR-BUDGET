using CielaDocs.Application.Models;

using Dapper;

using CielaDocs.Domain.Entities;

using CielaDocs.Application.Dtos;
using CielaDocs.Shared.DataAccess;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using System.Security.Cryptography;
using System.Data;
using static System.Net.Mime.MediaTypeNames;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Bibliography;
using System.Net.Http.Headers;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace CielaDocs.Shared.Repository
{
    public class SjcBudgetRepository : ISjcBudgetRepository
    {
        private SjcBudgetContext _context;

        public SjcBudgetRepository(SjcBudgetContext context)
        {
            this._context = context;
        }
        public async Task<IEnumerable<IdNames>> GetInstitutionsAsync() {
            string sql = $@"select Id,Name from InstitutionType";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql);
            return result?.ToList();
        }
        public async Task<IdNames> GetInstitutionByIdAsync(int? id)
        {
            string sql = $@"select Id,Name from InstitutionType where Id={id??0}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql);
            return result?.FirstOrDefault();
        }
        public async Task<IEnumerable<IdNames>> GetCourtTypeByInstitutionTypeIdAsync(int? institutionTypeId) {
            string sql = $@"select Id,Name from CourtType where InstitutionTypeId={institutionTypeId??0}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql);
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
        public async Task<IEnumerable<CourtsVm>> GetCourtsByCourtTypeIdAsync(int courtTypeId)
        {
            string sql = $@"select c.Id,c.Num,c.CourtTypeId,c.Name,c.IsActive,c.CourtGuid,c.KontoCode ,t.Name as CourtTypeName,i.Name as InstitutionTypeName
                            from Court c
                            join CourtType t on c.CourtTypeId=t.Id
                            join InstitutionType i on t.InstitutionTypeId=i.Id
                            where c.CourtTypeId=@CourtTypeId";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<CourtsVm>(sql, new { CourtTypeId = courtTypeId });
            return result?.ToList();
        }
        public async Task<CourtsVm> GetCourtByKontoCodeAsync(string kontoCode) {
            string sql = $@"select c.Id,c.Num,c.CourtTypeId,c.Name,c.IsActive,c.CourtGuid,c.KontoCode ,t.Name as CourtTypeName,i.Name as InstitutionTypeName
                            from Court c
                            join CourtType t on c.CourtTypeId=t.Id
                            join InstitutionType i on t.InstitutionTypeId=i.Id
                            where c.KontoCode='{kontoCode}'";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<CourtsVm>(sql);
            return result?.FirstOrDefault();
        }
        public async Task<CourtsVm> GetCourtByIdAsync(int id) {
            string sql = $@"select c.Id,c.Num,c.CourtTypeId,c.Name,c.IsActive,c.CourtGuid,c.KontoCode ,t.Name as CourtTypeName,i.Name as InstitutionTypeName
                            from Court c
                            join CourtType t on c.CourtTypeId=t.Id
                            join InstitutionType i on t.InstitutionTypeId=i.Id
                            where c.Id={id}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<CourtsVm>(sql);
            return result?.FirstOrDefault();
        }
        public async Task<IEnumerable<UsersVm>> GetUsersByCourtTypeIdAsync(int courtTypeId)
        {
            string sql = $@"select u.Id,u.CourtId, concat(u.FirstName,' ',u.MiddleName,' ',u.LastName) as UserFullName,u.Identifier,u.Email,u.LoginEnabled,u.UserTypeId,u.AspNetUserId,u.UserName,u.CanAdd,u.CanUpdate,u.CanDelete, t.Name as UserTypeName,c.Name as CourtName
                            from Users u
                            join UserType t on u.UserTypeId=t.Id
                            join Court c on u.CourtId=c.Id
                            where c.CourtTypeId=@CourtTypeId";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<UsersVm>(sql, new { CourtTypeId = courtTypeId });
            return result?.ToList();
        }
        public async Task<IEnumerable<UsersVm>> GetUsersWithoutAspNetUserIdAsync() {
            string sql = $@"select u.Id,u.CourtId, concat(u.FirstName,' ',u.MiddleName,' ',u.LastName) as UserFullName,u.Identifier,u.Email,u.LoginEnabled,u.UserTypeId,u.AspNetUserId,u.UserName,u.CanAdd,u.CanUpdate,u.CanDelete, t.Name as UserTypeName,c.Name as CourtName
                            from Users u
                            join UserType t on u.UserTypeId=t.Id
                            join Court c on u.CourtId=c.Id
                            where u.AspNetUserId=''";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<UsersVm>(sql);
            return result?.ToList();
        }
        public async Task<Domain.Entities.User> GetUserByIdAsync(int id) {
            string sql = $@"select * from Users where Id=@Id";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<Domain.Entities.User>(sql, new { Id = id });
            return result?.FirstOrDefault();
        }

        public async Task<Domain.Entities.User> GetUserByIdentifierAsync(string id)
        {
            string sql = $@"select top 1 * from Users where Identifier=@Identifier";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<Domain.Entities.User>(sql, new { Identifier = id });
            return result?.FirstOrDefault();
        }
        public async Task<Domain.Entities.User> GetUserByASpNetUserIdAsync(string aspNetUserId)
        {
            string sql = $@"select * from Users where AspNetUserId=@AspNetUserId";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<Domain.Entities.User>(sql, new { AspNetUserId = aspNetUserId });
            return result?.FirstOrDefault();
        }
        public async Task<IEnumerable<Domain.Entities.Section>> GetSectionsAsync() {
            string sql = $@"select * from Section ";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<Domain.Entities.Section>(sql);
            return result?.ToList();
        }
        public async Task<IEnumerable<EbkPar>> GetEbkParBySectionIdAsync(int sectionId)
        {
            string sql = $@"select * from EbkPar where sectionId=@SectionId";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<EbkPar>(sql, new { SectionId = sectionId });
            return result?.ToList();
        }
        public async Task<IEnumerable<EbkSubPar>> GetEbkSubParByEbkParIdAsync(int ebkParId)
        {
            string sql = $@"select * from EbkSubPar where ebkParId=@EbkParId";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<EbkSubPar>(sql, new { EbkParId = ebkParId });
            return result?.ToList();
        }
        public async Task<IEnumerable<TreeListMinDataModel>> GetTreeListEbkMinData() { 
            List<TreeListMinDataModel> result=new();
            int i = 0;
            var sec = await GetSectionsAsync();
            foreach (var section in sec)
            { 
                i++ ;
                result.Add(new TreeListMinDataModel {Id= i, Head_Id= 0,Name= (!string.IsNullOrWhiteSpace(section.Name))? section.Name: "Единна бюджетна класификация", DbId = section.Id, Db2Id=0 });
                
            }
            List<TreeListMinDataModel> newList = new List<TreeListMinDataModel>(result);
            foreach (var item in newList)
            {
                if (item?.DbId > 0)
                {
                    var ebks = await GetEbkParBySectionIdAsync(item.DbId);

                    if (ebks != null)
                    {
                        foreach (var ebk in ebks)
                        {
                            i++;
                            result.Add(new TreeListMinDataModel { Id = i, Head_Id = item.Id, Name = ebk.Name, DbId = 0, Db2Id = ebk.Id });
                        }
                    }
                }
            }
            List<TreeListMinDataModel> newList2 = new List<TreeListMinDataModel>(result);
            foreach (var item in newList2)
            {
                if (item?.Db2Id > 0)
                {
                    var ebks = await GetEbkSubParByEbkParIdAsync(item.Db2Id);

                    if (ebks != null)
                    {
                        foreach (var ebk in ebks)
                        {
                            i++;
                            result.Add(new TreeListMinDataModel { Id = i, Head_Id = item.Id, Name = ebk.Name, DbId = 0, Db2Id = 0});
                        }
                    }
                }
            }
            return result;
        }
        public async Task<IEnumerable<FunctionalArea>> GetFunctionalAreasAsync() {
            string sql = $@"select * from FunctionalArea where IsActive=1 order by id ";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<FunctionalArea>(sql);
            return result?.ToList();
        }
        public async Task<IEnumerable<FunctionalSubArea>> GetFunctionalSubAreasAsync(int functionalAreaId) {
            string sql = $@"select * from FunctionalSubArea where  IsActive=1 and FunctionalAreaId=@FunctionalAreaId";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<FunctionalSubArea>(sql, new { FunctionalAreaId = functionalAreaId });
            return result?.ToList();
        }
        public async Task<FunctionalSubArea> GetFunctionalSubAreabyIdAsync(int Id) {
            string sql = $@"select * from FunctionalSubArea where  Id=@Id";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<FunctionalSubArea>(sql, new { Id = Id });
            return result?.FirstOrDefault();
        }
        public async Task<IdNames> GetInstitutionTypeByIdAsync(int? typeId) {
            string sql = $@"select Id,Name from InstitutionType where  Id={typeId}";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql);
            return result?.FirstOrDefault();
        }
        public async Task<IEnumerable<TreeListMinDataModel>> GetTreeListFunctionalMinData()
        {
            List<TreeListMinDataModel> result = new();
            int rowId = 0; 
            var sec = await GetFunctionalAreasAsync();
            foreach (var section in sec)
            {
                rowId++;
                result.Add(new TreeListMinDataModel { Id = rowId, Head_Id = 0, Name = (!string.IsNullOrWhiteSpace(section.Name)) ? section.Name : "Функционална област" , DbId=section.Id});

            }
            List<TreeListMinDataModel> newList = new List<TreeListMinDataModel>(result);
            foreach (var item in newList)
            {
                if (item?.DbId > 0)
                {
                    var ebks = await GetFunctionalSubAreasAsync(item.DbId);

                    if (ebks != null)
                    {
                        foreach (var ebk in ebks)
                        {
                            rowId++;
                            result.Add(new TreeListMinDataModel { Id = rowId, Head_Id = item.Id, Name = ebk.Name, DbId = 0, Db2Id = ebk.Id });
                        }
                    }
                }
            }
            List<TreeListMinDataModel> newList2 = new List<TreeListMinDataModel>(result);
            foreach (var item in newList2)
            {
                if (item?.Db2Id > 0)
                {
                    var ebks = await GetFunctionalActionByProgramId(item.Db2Id);

                    if (ebks != null)
                    {
                        foreach (var ebk in ebks)
                        {
                            rowId++;
                            result.Add(new TreeListMinDataModel { Id = rowId, Head_Id = item.Id, Name = ebk.Name, DbId = 0, Db2Id = 0 });
                        }
                    }
                }
            }
            return result;
        }
        public async Task<IEnumerable<MainIndicatorsVm>> GetMainIndicatorsByProgramId(int programId) {
            string sql = $@"select a.Id,a.FunctionalSubAreaId,a.Code,a.Name,a.MeasureId,a.IsActive,a.Calculation,a.Gkey,f.Name as FunctionalSubAreaName, c.Name as MeasureName,t.Name as TypeOfIndicatorName
                            from MainIndicators a
                            join FunctionalSubArea f on a.FunctionalSubAreaId=f.Id
                            join Measure c on a.MeasureId=c.Id
                            join TypeOfIndicator t on a.TypeOfIndicatorId=t.id
                            where a.FunctionalSubAreaId=@FunctionalSubAreaId";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<MainIndicatorsVm>(sql, new { FunctionalSubAreaId = programId });
            return result?.ToList();
        }
        public async Task<IEnumerable<ProgramDefVm>> GetProgramDefByProgramIdAsync(int programId) {
                        string sql = $@"SELECT Id,FunctionalAreaId,FunctionalSubAreaId,FunctionalActionId,RowNum,RowCode,PrnCode,Name,ParentRowNum
                              ,Nvalue,EnteredDate,CurrencyId,CurrencyMeasureId,Datum,ValueAllowed,Num,IsActive,OrderNum,KontoCodes,Notes,IsCalculated,ProgCode
                          FROM dbo.ProgramDef  where FunctionalSubAreaId=@FunctionalSubAreaId";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDefVm>(sql, new { FunctionalSubAreaId = programId });
            return result?.ToList();
        }
        public async Task<IEnumerable<ProgramDefVm>> GetProgramDefProgCodesByProgramIdAsync(int programId) {
            string sql = $@"SELECT Id,FunctionalAreaId,FunctionalSubAreaId,FunctionalActionId,RowNum,RowCode,PrnCode,Name,ParentRowNum
                              ,Nvalue,EnteredDate,CurrencyId,CurrencyMeasureId,Datum,ValueAllowed,Num,IsActive,OrderNum,KontoCodes,Notes,IsCalculated,ProgCode
                          FROM dbo.ProgramDef  where FunctionalSubAreaId=@FunctionalSubAreaId and ProgCode<>'' ";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDefVm>(sql, new { FunctionalSubAreaId = programId });
            return result?.ToList();
        }
        public async Task<CourtTypeVm> GetCourtTypeVmByIdAsync(int? id) {
            string sql = $@"SELECT top 1 Id,Name,InstitutionTypeId  FROM dbo.CourtType  where Id=@Id";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<CourtTypeVm>(sql, new { Id = id });
            return result?.FirstOrDefault();
        }
        public async Task<ProgramDefVm> GetProgramDefByIdAsync(int id)
        {
            string sql = $@"SELECT top 1 Id,FunctionalAreaId,FunctionalSubAreaId,FunctionalActionId,RowNum,RowCode,PrnCode,Name,ParentRowNum
                              ,Nvalue,EnteredDate,CurrencyId,CurrencyMeasureId,Datum,ValueAllowed,Num,IsActive,OrderNum,KontoCodes,Notes,IsCalculated,ProgCode
                          FROM dbo.ProgramDef  where Id=@Id";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDefVm>(sql, new { Id = id });
            return result?.FirstOrDefault();
        }

        public async Task<IEnumerable<CourtInProgramVm>> GetCourtInProgramByCourtIdAsync(int? courtId) {
            string sql = $@"SELECT  1 Id,CourtId,FunctionalSubAreaId  FROM dbo.CourtInProgram  where CourtId={courtId??0}";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<CourtInProgramVm>(sql);
            return result?.ToList();
        }
        public async Task<IEnumerable<InstitutionInProgramVm>> GetInstitutionInProgramByInstitutionTypeIdAsync(int? institutionTypeId)
        {
            string sql = $@"SELECT  1 Id,InstitutionTypeId,FunctionalSubAreaId  FROM dbo.InstitutionInProgram  where InstitutionTypeId={institutionTypeId ?? 0}";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<InstitutionInProgramVm>(sql);
            return result?.ToList();
        }
        public async Task<IEnumerable<IdNames>> GetProgramByCourtIdAsync(int? courtId) {
            string sql = $@"select Id,Name FROM dbo.FunctionalSubArea  where Id in(select distinct(FunctionalSubAreaId) from CourtInProgram where CourtId={courtId??0})";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql);
            return result?.ToList();
        }
        public async Task<IEnumerable<IdNames>> GetAllProgramsAsync()
        {
            string sql = $@"select Id,Name FROM dbo.FunctionalSubArea";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql);
            return result?.ToList();
        }
        public async Task<IEnumerable<IdNames>> GetProgramByInstitutionTypeIdAsync(int? institutionTypeId)
        {
            string sql = $@"select Id,Name FROM dbo.FunctionalSubArea  where Id in(select distinct(FunctionalSubAreaId) from InstitutionInProgram where InstitutionTypeId={institutionTypeId ?? 0})";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql);
            return result?.ToList();
        }
        public async Task<IEnumerable<IdNames>> GetProgramsAsync()
        {
            string sql = $@"select Id,Name FROM dbo.FunctionalSubArea";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql);
            return result?.ToList();
        }
        public async Task<IEnumerable<FinYear>> GetFinYear()
        {
            string sql = $@"select * from FinYear order by id ";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<FinYear>(sql);
            return result?.ToList();
        }
        public async Task<IEnumerable<FunctionalAction>> GetFunctionalActionByProgramId(int programId) {
            string sql = $@"select * from FunctionalAction
                            where FunctionalSubAreaId=@FunctionalSubAreaId";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<FunctionalAction>(sql, new { FunctionalSubAreaId = programId });
            return result?.ToList();
        }
        public async Task<IEnumerable<Test>> GetTestTableAsync() {
            string sql = $@"select * from test";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<Test>(sql);
            return result?.ToList();
        }
        public async Task<IEnumerable<Test2>> GetTest2TableAsync()
        {
            string sql = $@"select * from test2";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<Test2>(sql);
            return result?.ToList();
        }
        public async Task<int> UpdateTestValue(int id,decimal val)
        {
            var sql = @"UPDATE test SET Nvalue = @Nvalue WHERE Id = @Id";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql,new {Nvalue=val, Id=id });

            return affectedRows.Result;
           
        }
       
        public async Task<int> UpdateTest2Value(int id, decimal val)
        {
            var sql = @"UPDATE test2 SET Nvalue = @Nvalue WHERE Id = @Id";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql, new { Nvalue = val, Id = id });

            return affectedRows.Result;

        }
        public async Task<int> UpdateUserWithAspNetUserIdAsync(int id, string s)
        {
            var sql = $@"UPDATE Users SET AspNetUserId = '{s}' WHERE Id = {id}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql);

            return affectedRows.Result;
        }
        public async Task<IEnumerable<Metrics>> GetMetricsByProgramId(int programId) {
            string sql = $@"select * from Metrics
                            where FunctionalSubAreaId=@FunctionalSubAreaId";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<Metrics>(sql, new { FunctionalSubAreaId = programId });
            return result?.ToList();
        }
        public async Task<IEnumerable<MetricsFieldVm>> GetMetricsFields() {
            string sql = $@"select m.Id,m.Code,m.Name,m.NeededFor,m.IsActive,m.TypeOfIndicatorId, t.Name as TypeOfIndicatorName from MetricsField m
                            left join TypeOfIndicator t on m.TypeOfIndicatorId=t.id
                            order by m.id";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<MetricsFieldVm>(sql);
            return result?.ToList();
        }
        public async Task<IEnumerable<IdNames>> GetMeasureAsync() {
            string sql = $@"select Id,Name from Measure order by id";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql);
            return result?.ToList();
        }
        public async Task<IEnumerable<IdNames>> GetTypeOfIndicatorAsync()
        {
            string sql = $@"select Id,Name from TypeOfIndicator order by id";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql);
            return result?.ToList();
        }
        public async Task<IdNames> GetMeasureByIdAsync(int id)
        {
            string sql = $@"select Id,Name from Measure  where Id=@Id";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql, new { Id = id });
            return result?.FirstOrDefault();
        }
        public async Task<IdNames> GetTypeOfIndicatorByIdAsync(int id)
        {
            string sql = $@"select Id,Name from TypeOfIndicator where Id=@Id";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql, new { Id=id});
            return result?.FirstOrDefault();
        }
        public async Task<bool> CheckMainDataByCourtIdPeriodAsync(int courtId, int nm, int ny) {
            var sql = "SELECT top 1 Id FROM MainData WHERE CourtId=@CourtId and NMonth=@NMonth  and NYear=@NYear";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<int>(sql, new { CourtId = courtId, NMonth = nm, NYear=ny });
            return result.Any();
        }
        public async Task<bool> CheckMainDataItemsByCourtIdPeriodAsync(int courtId, int nm, int ny) {
            var sql = "SELECT top 1 Id FROM MainDataItems WHERE CourtId=@CourtId and NMonth=@NMonth  and NYear=@NYear";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<int>(sql, new { CourtId = courtId, NMonth = nm, NYear = ny });
            return result.Any();
        }
        public async Task<bool> CheckPeriodDataByCourtIdPeriodAsync(int courtId, int nm, int ny) {
            var sql = "SELECT top 1 Id FROM MainPeriod WHERE CourtId=@CourtId and NMonth=@NMonth  and NYear=@NYear";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<int>(sql, new { CourtId = courtId, NMonth = nm, NYear = ny });
            return result.Any();
        }
        public async Task<bool> CheckPeriodDataItemsByCourtIdPeriodAsync(int courtId, int nm, int ny) {
            var sql = "SELECT top 1 Id FROM MainPeriodItems WHERE CourtId=@CourtId and NMonth=@NMonth  and NYear=@NYear";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<int>(sql, new { CourtId = courtId, NMonth = nm, NYear = ny });
            return result.Any();
        }
        public async Task<int?> SpLoadMainDataByCourtIdPeriodAsync(int courtId, int nm, int ny) {
         
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("CourtId", courtId);
            parameters.Add("NMonth", nm);
            parameters.Add("NYear", ny);
            var ret = connection.ExecuteAsync("sp_LoadMainData", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }
        public async Task<int?> SpLoadMainDataItemsByCourtIdPeriodAsync(int courtId, int nm, int ny)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("CourtId", courtId);
            parameters.Add("NMonth", nm);
            parameters.Add("NYear", ny);
            var ret = connection.ExecuteAsync("sp_LoadMainDataItems", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }
        public async Task<int?> SpLoadMainPeriodByCourtIdPeriodAsync(int courtId, int nm, int ny)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("CourtId", courtId);
            parameters.Add("NMonth", nm);
            parameters.Add("NYear", ny);
            var ret = connection.ExecuteAsync("sp_LoadPeriodData", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }
        public async Task<int?> SpLoadMainPeriodItemsByCourtIdPeriodAsync(int courtId, int nm, int ny)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("CourtId", courtId);
            parameters.Add("NMonth", nm);
            parameters.Add("NYear", ny);
            var ret = connection.ExecuteAsync("sp_LoadMainPeriodItems", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }
        public async Task<IEnumerable<MainDataGrid>> GetMainDataGridByFilterAsync(int functionalSubAreaId, int courtId, int nm, int ny) {
            string sql = $@"select m.Id,m.CourtId,m.NMonth,m.NYear,m.MainIndicatorsId,m.Nvalue,m.EnteredValue,m.Datum,m.EnteredOn,i.Name as MainIndicatorName,i.Code,i.MeasureId,i.TypeOfIndicatorId,i.Calculation,c.Name as MeasureName,t.Name as TypeOfIndicatorName
                        from MainData m
                        join MainIndicators i on m.MainIndicatorsId=i.Id
                        join Measure c on i.MeasureId=c.Id
                        join TypeOfIndicator t on i.TypeOfIndicatorId=t.id
                        Where m.CourtId=@CourtId and m.NMonth=@NMonth and m.Nyear=@NYear and i.FunctionalSubAreaId=@FunctionalSubAreaId ";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<MainDataGrid>(sql, new { FunctionalSubAreaId = functionalSubAreaId, CourtId= courtId, NMonth=nm, NYear=ny });
            return result?.ToList();
        }
        public async Task<IEnumerable<MainDataGrid>> GetMainPeriodGridByFilterAsync(int functionalSubAreaId, int courtId, int nm, int ny)
        {
            string sql = $@"select m.Id,m.CourtId,m.NMonth,m.NYear,m.MainIndicatorsId,m.Nvalue,m.EnteredValue,m.Datum,m.EnteredOn,i.Name as MainIndicatorName,i.Code,i.MeasureId,i.TypeOfIndicatorId,i.Calculation,c.Name as MeasureName,t.Name as TypeOfIndicatorName
                        from MainPeriod m
                        join MainIndicators i on m.MainIndicatorsId=i.Id
                        join Measure c on i.MeasureId=c.Id
                        join TypeOfIndicator t on i.TypeOfIndicatorId=t.id
                        Where m.CourtId=@CourtId and m.NMonth=@NMonth and m.Nyear=@NYear and i.FunctionalSubAreaId=@FunctionalSubAreaId ";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<MainDataGrid>(sql, new { FunctionalSubAreaId = functionalSubAreaId, CourtId = courtId, NMonth = nm, NYear = ny });
            return result?.ToList();
        }

        public async Task<MainData> GetMainDataByIdAsync(int Id)
        {
            string sql = $@"select top 1  * from MainData where Id=@Id";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<MainData>(sql, new { Id = Id });
            return result?.FirstOrDefault();
        }

        public async Task<MainData> GetMainDataPeriodByIdAsync(int Id)
        {
            string sql = $@"select top 1  * from MainPeriod where Id=@Id";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<MainData>(sql, new { Id = Id });
            return result?.FirstOrDefault();
        }
        public async Task<MainIndicators> GetMainIndicatorsByIdAsync(int Id)
            {
                string sql = $@"select top 1  * from MainIndicators where Id=@Id";

                await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
                await connection.OpenAsync();
                var result = await connection.QueryAsync<MainIndicators>(sql, new { Id = Id });
                return result?.FirstOrDefault();
        }
        public async Task<IEnumerable<MainDataItemsVm>> GetMetricsFiledByCode(int courtId,int nm, int ny, string codes) {
           
            var st=string.Empty;
            foreach (var code in codes.Split(','))
            {
                st+=$"'{code}',";
            }
            st = st.Remove(st.LastIndexOf(","), 1);


            string sql2 = $@"select m.Id
                  ,m.CourtId
                  ,m.NMonth
                  ,m.NYear
                  ,m.MetricsFieldId
                  ,m.Nvalue
                  ,a.Code
                  ,a.Name as MetricsFieldName
            from MainDataItems m
            join MetricsField a on m.MetricsFieldId=a.Id
            where m.CourtId={courtId} and m.NMonth={nm} and m.NYear={ny} and a.code in({st})";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<MainDataItemsVm> (sql2);
            return result?.ToList();
        }
        public async Task<IEnumerable<MainDataItemsVm>> GetMainDataItemFiledByCodes(int courtId, int nm, int ny, string codes) {
            var st = string.Empty;
            foreach (var code in codes.Split(','))
            {
                st += $"'{code}',";
            }
            st = st.Remove(st.LastIndexOf(","), 1);
            string sql2 = $@"select m.Id
                  ,m.CourtId
                  ,m.NMonth
                  ,m.NYear
                  ,m.MetricsFieldId
                  ,m.Nvalue
                  ,a.Code
                  ,a.Name as MetricsFieldName
            from MainDataItems m
            join MetricsField a on m.MetricsFieldId=a.Id
            where m.CourtId={courtId} and m.NMonth={nm} and m.NYear={ny} and a.code in({st})";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<MainDataItemsVm>(sql2);
            return result?.ToList();
        }
        public async Task<IEnumerable<MainDataItemsVm>> GetMainPeriodItemFiledByCodes(int courtId, int nm, int ny, string codes)
        {
            var st = string.Empty;
            foreach (var code in codes.Split(','))
            {
                st += $"'{code}',";
            }
            st = st.Remove(st.LastIndexOf(","), 1);
            string sql2 = $@"select m.Id
                  ,m.CourtId
                  ,m.NMonth
                  ,m.NYear
                  ,m.MetricsFieldId
                  ,m.Nvalue
                  ,a.Code
                  ,a.Name as MetricsFieldName
            from MainPeriodItems m
            join MetricsField a on m.MetricsFieldId=a.Id
            where m.CourtId={courtId} and m.NMonth={nm} and m.NYear={ny} and a.code in({st} )";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<MainDataItemsVm>(sql2);
            return result?.ToList();
        }
        public async Task<int> UpdateMainDataItemByIdAsync(IEnumerable<MainDataItemsResult> data) {
            if (data.Any()) {
               
                foreach (var item in data)
                {
                    await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
                    await connection.OpenAsync();
                    var sql = $@"UPDATE MainDataItems SET Nvalue = {item.Nvalue}, EnteredOn=getDate()  WHERE Id = {item.Id}";
                    var affectedRows = connection.ExecuteAsync(sql);
                   
                }
                return 1;
            }
            else { return 0; }
        }
        public async Task<int> UpdateMainDataValueByIdAsync(int? Id, double? nValue) {
            var sql = $@"UPDATE MainData SET Nvalue ={nValue??0}, EnteredOn=getDate()  WHERE Id = {Id??0}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql);
            return affectedRows.Result;
        }
        public async Task<IEnumerable<MainDataItemsGrid>> GetMainDataItemsGridByFilterAsync(int courtId, int nm, int ny) {
            string sql2 = $@"select m.Id
                  ,m.CourtId
                  ,m.NMonth
                  ,m.NYear
                  ,m.MetricsFieldId
                  ,m.Nvalue
                  ,m.Datum
                  ,m.EnteredOn
                  ,a.Code
                  ,a.Name
            from MainDataItems m
            join MetricsField a on m.MetricsFieldId=a.Id
            where m.CourtId={courtId} and m.NMonth={nm} and m.NYear={ny}";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<MainDataItemsGrid>(sql2);
            return result?.ToList();
        }
        public async Task<int> UpdateMainPeriodValueByIdAsync(int? Id, double? nValue)
        {
            var sql = $@"UPDATE MainPeriod SET Nvalue ={nValue ?? 0}, EnteredOn=getDate()  WHERE Id = {Id ?? 0}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql);
            return affectedRows.Result;
        }
       
        public async Task<IEnumerable<MainDataItemsGrid>> GetMainPeriodItemsGridByFilterAsync(int courtId, int nm, int ny)
        {
            string sql2 = $@"select m.Id
                  ,m.CourtId
                  ,m.NMonth
                  ,m.NYear
                  ,m.MetricsFieldId
                  ,m.Nvalue
                  ,m.Datum
                  ,m.EnteredOn
                  ,a.Code
                  ,a.Name
            from MainPeriodItems m
            join MetricsField a on m.MetricsFieldId=a.Id
            where m.CourtId={courtId} and m.NMonth={nm} and m.NYear={ny}";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<MainDataItemsGrid>(sql2);
            return result?.ToList();
        }
        public async Task<int> UpdateMainDataItemValueByIdAsync(int? id, decimal? val)
        {
            var sql = @"UPDATE MainDataItems SET Nvalue = @Nvalue, EnteredOn=getDate() WHERE Id = @Id";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql, new { Nvalue = val, Id = id });

            return affectedRows.Result;

        }
        public async Task<int> UpdateMainPeriodItemValueByIdAsync(int? id, decimal? val)
        {
            var sql = @"UPDATE MainPeriodItems SET Nvalue = @Nvalue, EnteredOn=getDate() WHERE Id = @Id";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql, new { Nvalue = val, Id = id });

            return affectedRows.Result;

        }
        public async Task<int> UpdateMainDataEnteredValueByIdAsync(int? id, decimal? val)
        {
            var sql = @"UPDATE MainData SET EnteredValue = @EnteredValue, EnteredOn=getDate() WHERE Id = @Id";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql, new { EnteredValue = val, Id = id });

            return affectedRows.Result;

        }
        public async Task<int> UpdateMainPeriodEnteredValueByIdAsync(int? id, decimal? val)
        {
            var sql = @"UPDATE MainPeriod SET EnteredValue = @EnteredValue, EnteredOn=getDate() WHERE Id = @Id";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql, new { EnteredValue = val, Id = id });

            return affectedRows.Result;

        }
        public async Task<int?> Sp_InitFinYearStage1Async(int ny) {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("nYear", ny);
            var ret = connection.ExecuteAsync("sp_InitFinYear1", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }
        public async Task<int?> Sp_InitFinYearStage2Async(int ny)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("nYear", ny);
            var ret = connection.ExecuteAsync("sp_InitFinYear2", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }

        public async Task<int?> Sp_InitProgramDataAsync(int? programNum, int? ny)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("ProgramDefNum", programNum??0);
            parameters.Add("nYear", ny);
            var ret = connection.ExecuteAsync("sp_InitProgramData", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }
        public async Task<int?> Sp_InitProgramDataCourtAsync(int? programNum, int? ny)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("ProgramDefNum", programNum ?? 0);
            parameters.Add("nYear", ny);
            var ret = connection.ExecuteAsync("sp_InitProgramDataCourt", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }
        public async Task<int?> Sp_InitProgramDataInstitutionAsync(int? programNum, int? ny)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("ProgramDefNum", programNum ?? 0);
            parameters.Add("nYear", ny);
            var ret = connection.ExecuteAsync("sp_InitProgramDataInstitution", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }
        public async Task<int?> Sp_UpdateProgramDataAsync(int? programNum, int? ny)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("ProgramDefNum", programNum ?? 0);
            parameters.Add("nYear", ny);
            var ret = connection.ExecuteAsync("sp_UpdateProgramData", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }
        public async Task<int?> Sp_UpdateProgramDataCourtAsync(int? programNum, int? ny)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("ProgramDefNum", programNum ?? 0);
            parameters.Add("nYear", ny);
            var ret = connection.ExecuteAsync("sp_UpdateProgramDataCourt", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }
        public async Task<int?> Sp_UpdateProgramDataInstitutionAsync(int? programNum, int? ny)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("ProgramDefNum", programNum ?? 0);
            parameters.Add("nYear", ny);
            var ret = connection.ExecuteAsync("sp_UpdateProgramDataInstitution", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }
        public async Task<IEnumerable<ProgramDataGridVm>> GetProgramDataGridByFilterAsync(int functionalSubAreaId, int ny)
        {
            string sql2 = $@"select p.Id,
	                p.ProgramDefNum,
	                p.FunctionalAreaId,
	                p.FunctionalSubAreaId,
	                p.FunctionalActionId,
	                p.RowNum,
	                p.RowCode,
	                p.PrnCode,
	                p.Name,
	                p.ParentRowNum,
	                p.Nvalue,
	                p.EnteredDate,
	                p.CurrencyId,
	                p.CurrencyMeasureId,
	                p.Datum,
	                p.ValueAllowed,
	                p.PlannedYear,
	                p.IsActive,
	                p.OrderNum,
	                p.ApprovedValue,
	                p.CalculatedValue,
                    p.IsCalculated,
                    a.Name as FunctionalSubAreaName,
                    c.Name as CurrencyName,
                    m.Name as CurrencyMeasureName

             from ProgramData p
             left join FunctionalSubArea a on p.FunctionalSubAreaId=a.Id
             left join Currency c on p.CurrencyId=c.Id
             left join CurrencyMeasure m on p.CurrencyMeasureId=m.Id
             where p.FunctionalSubAreaId={functionalSubAreaId} and p.PlannedYear={ny} and p.IsActive=1";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDataGridVm>(sql2);
            return result?.ToList();
        }
        public async Task<IEnumerable<ProgramData3Y>> GetProgramData3YAsync(int functionalSubAreaId, int ny) {
            string sql2 = $@"select a.Id,a.FunctionalSubAreaId,a.RowNum,a.PlannedYear,a.PrnCode,a.Name,a.ValueAllowed,a.Nvalue as nval1,b.nvalue as nval2,c.Nvalue as nval3
	              from ProgramData a 
	              left join ProgramData b on a.FunctionalSubAreaId=b.FunctionalSubAreaId and a.RowNum=b.RowNum and b.PlannedYear=a.PlannedYear+1
	              left join ProgramData c on a.FunctionalSubAreaId=c.FunctionalSubAreaId and a.RowNum=c.RowNum and c.PlannedYear=a.PlannedYear+2

	              where a.FunctionalSubAreaId={functionalSubAreaId} and a.PlannedYear={ny} and a.IsActive=1";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramData3Y>(sql2);
            return result?.ToList();
        }
        public async Task<IEnumerable<ProgramDataCourt3Y>> GetProgramDataCourt3YAsync(int functionalSubAreaId, int ny, int? rowNum) {
            string sql2 = $@"select a.Id,a.CourtId,a.FunctionalSubAreaId,a.RowNum,a.PlannedYear,a.PrnCode,a.Name,a.ValueAllowed,a.Nvalue as nval1,b.nvalue as nval2,c.Nvalue as nval3, co.Name as CourtName
	              from ProgramDataCourt a 
	              left join ProgramDataCourt b on a.FunctionalSubAreaId=b.FunctionalSubAreaId and a.RowNum=b.RowNum and a.CourtId=b.CourtId and b.PlannedYear=a.PlannedYear+1
	              left join ProgramDataCourt c on a.FunctionalSubAreaId=c.FunctionalSubAreaId and a.RowNum=c.RowNum and a.CourtId=c.CourtId and c.PlannedYear=a.PlannedYear+2 

                  left join Court co on a.CourtId=co.Id

	              where a.FunctionalSubAreaId={functionalSubAreaId} and a.PlannedYear={ny} and a.RowNum={rowNum??0} and a.IsActive=1";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDataCourt3Y>(sql2);
            return result?.ToList();
        }
        public async Task<IEnumerable<ProgramDataCourt3Y>> GetProgramDataCourt3YByCourtIdAsync(int? programDefNum, int? ny, int? courtId) {
            string sql2 = $@"select a.Id,a.CourtId,a.FunctionalSubAreaId,a.RowNum,a.PlannedYear,a.PrnCode,a.Name,a.ValueAllowed,a.Nvalue as nval1,b.nvalue as nval2,c.Nvalue as nval3, co.Name as CourtName
	              from ProgramDataCourt a 
	              left join ProgramDataCourt b on a.FunctionalSubAreaId=b.FunctionalSubAreaId and a.RowNum=b.RowNum and a.CourtId=b.CourtId and b.PlannedYear=a.PlannedYear+1
	              left join ProgramDataCourt c on a.FunctionalSubAreaId=c.FunctionalSubAreaId and a.RowNum=c.RowNum and a.CourtId=c.CourtId and c.PlannedYear=a.PlannedYear+2 

                  left join Court co on a.CourtId=co.Id

	              where a.FunctionalSubAreaId={programDefNum??0} and a.PlannedYear={ny} and a.CourtId={courtId ?? 0} and a.IsActive=1";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDataCourt3Y>(sql2);
            return result?.ToList();
        }
        public async Task<IEnumerable<ProgramDef3Y>> GetProgramDataCourt3YCommonAsync(int? programDefNum, int? ny)
        {
            string sql2 = $@"select a.Id,a.FunctionalSubAreaId,a.RowNum,a.PrnCode,a.Name,t1.Nval1, t2.Nval2,t3.Nval3 
	             from ProgramDef a 
                 left join    (
                                  select  FunctionalSubAreaId,RowNum,PlannedYear,Sum(Nvalue) as Nval1
                                  from    ProgramDataCourt
                                  group by
                                          FunctionalSubAreaId,RowNum,PlannedYear
                                  ) t1
                          on      t1.FunctionalSubAreaId=a.FunctionalSubAreaId and t1.RowNum=a.RowNum and t1.PlannedYear={ny}

				  left join    (
                                  select  FunctionalSubAreaId,RowNum,PlannedYear,Sum(Nvalue) as Nval2
                                  from    ProgramDataCourt
                                  group by
                                          FunctionalSubAreaId,RowNum,PlannedYear
                                  ) t2
                          on      t2.FunctionalSubAreaId=a.FunctionalSubAreaId and t2.RowNum=a.RowNum and t2.PlannedYear={ny+1}
				  left join    (
                                  select  FunctionalSubAreaId,RowNum,PlannedYear,Sum(Nvalue) as Nval3
                                  from    ProgramDataCourt
                                  group by
                                          FunctionalSubAreaId,RowNum,PlannedYear
                                  ) t3
                          on      t3.FunctionalSubAreaId=a.FunctionalSubAreaId and t3.RowNum=a.RowNum and t3.PlannedYear={ny+2}

	              where a.FunctionalSubAreaId={programDefNum ?? 0}  and a.IsActive=1";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDef3Y>(sql2);
            return result?.ToList();
        }
        public async Task<IEnumerable<ProgramDef3Y>> GetProgramDataInstitution3YCommonAsync(int? programDefNum, int? ny)
        {
            string sql2 = $@"select a.Id,a.FunctionalSubAreaId,a.RowNum,a.PrnCode,a.Name,t1.Nval1, t2.Nval2,t3.Nval3 
	             from ProgramDef a 
                 left join    (
                                  select  FunctionalSubAreaId,RowNum,PlannedYear,Sum(Nvalue) as Nval1
                                  from   ProgramDataInstitution
                                  group by
                                          FunctionalSubAreaId,RowNum,PlannedYear
                                  ) t1
                          on      t1.FunctionalSubAreaId=a.FunctionalSubAreaId and t1.RowNum=a.RowNum and t1.PlannedYear={ny}

				  left join    (
                                  select  FunctionalSubAreaId,RowNum,PlannedYear,Sum(Nvalue) as Nval2
                                  from    ProgramDataInstitution
                                  group by
                                          FunctionalSubAreaId,RowNum,PlannedYear
                                  ) t2
                          on      t2.FunctionalSubAreaId=a.FunctionalSubAreaId and t2.RowNum=a.RowNum and t2.PlannedYear={ny + 1}
				  left join    (
                                  select  FunctionalSubAreaId,RowNum,PlannedYear,Sum(Nvalue) as Nval3
                                  from    ProgramDataInstitution
                                  group by
                                          FunctionalSubAreaId,RowNum,PlannedYear
                                  ) t3
                          on      t3.FunctionalSubAreaId=a.FunctionalSubAreaId and t3.RowNum=a.RowNum and t3.PlannedYear={ny + 2}

	              where a.FunctionalSubAreaId={programDefNum ?? 0}  and a.IsActive=1";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDef3Y>(sql2);
            return result?.ToList();
        }
        public async Task<IEnumerable<ProgramDataInstitution3Y>> GetProgramDataInstitution3YByInstitutionTypeIdAsync(int? programDefNum, int? ny, int? institutionTypeId)
        {
            string sql2 = $@"select a.Id,a.InstitutionTypeId,a.FunctionalSubAreaId,a.RowNum,a.PlannedYear,a.PrnCode,a.Name,a.ValueAllowed,a.Nvalue as nval1,b.nvalue as nval2,c.Nvalue as nval3, co.Name as CourtName
	              from ProgramDataInstitution a 
	              left join ProgramDataInstitution b on a.FunctionalSubAreaId=b.FunctionalSubAreaId and a.RowNum=b.RowNum and a.InstitutionTypeId=b.InstitutionTypeId and b.PlannedYear=a.PlannedYear+1
	              left join ProgramDataInstitution c on a.FunctionalSubAreaId=c.FunctionalSubAreaId and a.RowNum=c.RowNum and a.InstitutionTypeId=c.InstitutionTypeId and c.PlannedYear=a.PlannedYear+2 

                  left join InstitutionType co on a.InstitutionTypeId=co.Id

	              where a.FunctionalSubAreaId={programDefNum ?? 0} and a.PlannedYear={ny} and a.InstitutionTypeId={institutionTypeId ?? 0} and a.IsActive=1";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDataInstitution3Y>(sql2);
            return result?.ToList();
        }
        private async Task<decimal> GetTotalNvalue(int? functionalSubAreaId, int? rowNum, int? institutionTypeId, int? plannedYear) {
            string sql2 = $@"select sum(nvalue) from ProgramDataCourt where  FunctionalSubAreaId={functionalSubAreaId ?? 0} and RowNum={rowNum ?? 0} and CourtId in(select Id from court where courtTypeId in(select Id from courtType where InstitutionTypeId={institutionTypeId ?? 0} )) and PlannedYear={plannedYear ?? 0}";
	            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<decimal?>(sql2);
            return result?.FirstOrDefault()??0;
        }
        public async Task<IEnumerable<ProgramData3Y>> GetProgramData3YTotalAsync(int? programDefNum, int? ny, int? institutionTypeId)
        {

            List<ProgramData3Y> res = new();

            var pdef = await GetProgramDefByProgramIdAsync(programDefNum ?? 0);
            foreach (var item in pdef) {
                ProgramData3Y obj = new ProgramData3Y();
                obj.FunctionalSubAreaId = item.FunctionalSubAreaId;
                obj.RowNum= item.RowNum;
                obj.PrnCode= item.PrnCode;
                obj.Name= item.Name;
                obj.Nval1 = await GetTotalNvalue(item.FunctionalSubAreaId, item.RowNum,institutionTypeId, ny);
                obj.Nval2 = await GetTotalNvalue(item.FunctionalSubAreaId, item.RowNum, institutionTypeId, ny+1);
                obj.Nval3 = await GetTotalNvalue(item.FunctionalSubAreaId, item.RowNum, institutionTypeId, ny+2);
                res.Add(obj);
            }

          
            return res;
        }
        public async Task<ProgramDataGridVm> GetProgramDataByIdAsync(int? id){
            string sql2 = $@"select p.Id,
	                p.ProgramDefNum,
	                p.FunctionalAreaId,
	                p.FunctionalSubAreaId,
	                p.FunctionalActionId,
	                p.RowNum,
	                p.RowCode,
	                p.PrnCode,
	                p.Name,
	                p.ParentRowNum,
	                p.Nvalue,
	                p.EnteredDate,
	                p.CurrencyId,
	                p.CurrencyMeasureId,
	                p.Datum,
	                p.ValueAllowed,
	                p.PlannedYear,
	                p.IsActive,
	                p.OrderNum,
	                p.ApprovedValue,
	                p.CalculatedValue,
                    p.IsCalculated,
                    a.Name as FunctionalSubAreaName,
                    c.Name as CurrencyName,
                    m.Name as CurrencyMeasureName

             from ProgramData p
             left join FunctionalSubArea a on p.FunctionalSubAreaId=a.Id
             left join Currency c on p.CurrencyId=c.Id
             left join CurrencyMeasure m on p.CurrencyMeasureId=m.Id
             where p.Id={id??0}";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDataGridVm>(sql2);
            return result?.FirstOrDefault();
        }
        public async Task<ProgramDataCourtGridVm> GetProgramDataCourtByIdAsync(int? id) {
            string sql2 = $@"select p.Id,
                    p.CourtId,
	                p.ProgramDefNum,
	                p.FunctionalAreaId,
	                p.FunctionalSubAreaId,
	                p.FunctionalActionId,
	                p.RowNum,
	                p.RowCode,
	                p.PrnCode,
	                p.Name,
	                p.ParentRowNum,
	                p.Nvalue,
	                p.EnteredDate,
	                p.CurrencyId,
	                p.CurrencyMeasureId,
	                p.Datum,
	                p.ValueAllowed,
	                p.PlannedYear,
	                p.IsActive,
	                p.OrderNum,
	                p.ApprovedValue,
	                p.CalculatedValue,
                    p.IsCalculated,
                    a.Name as FunctionalSubAreaName,
                    c.Name as CurrencyName,
                    m.Name as CurrencyMeasureName

             from ProgramDataCourt p
             left join FunctionalSubArea a on p.FunctionalSubAreaId=a.Id
             left join Currency c on p.CurrencyId=c.Id
             left join CurrencyMeasure m on p.CurrencyMeasureId=m.Id
             where p.Id={id ?? 0}";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDataCourtGridVm>(sql2);
            return result?.FirstOrDefault();
        }
        public async Task<ProgramDataInstitutionGridVm> GetProgramDataInstitutionByIdAsync(int? id)
        {
            string sql2 = $@"select p.Id,
                    p.InstitutionTypeId,
	                p.ProgramDefNum,
	                p.FunctionalAreaId,
	                p.FunctionalSubAreaId,
	                p.FunctionalActionId,
	                p.RowNum,
	                p.RowCode,
	                p.PrnCode,
	                p.Name,
	                p.ParentRowNum,
	                p.Nvalue,
	                p.EnteredDate,
	                p.CurrencyId,
	                p.CurrencyMeasureId,
	                p.Datum,
	                p.ValueAllowed,
	                p.PlannedYear,
	                p.IsActive,
	                p.OrderNum,
	                p.ApprovedValue,
	                p.CalculatedValue,
                    p.IsCalculated,
                    a.Name as FunctionalSubAreaName,
                    c.Name as CurrencyName,
                    m.Name as CurrencyMeasureName

             from ProgramDataInstitution p
             left join FunctionalSubArea a on p.FunctionalSubAreaId=a.Id
             left join Currency c on p.CurrencyId=c.Id
             left join CurrencyMeasure m on p.CurrencyMeasureId=m.Id
             where p.Id={id ?? 0}";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDataInstitutionGridVm>(sql2);
            return result?.FirstOrDefault();
        }
        public async Task<IEnumerable<ProgramDataCourtGridVm>> GetProgramDataCourtGridByFilterAsync(int? programDefNum, int? ny, int? rowNum) {
            string sql2 = $@"select p.Id,
                    p.CourtId,
	                p.ProgramDefNum,
	                p.FunctionalAreaId,
	                p.FunctionalSubAreaId,
	                p.FunctionalActionId,
	                p.RowNum,
	                p.RowCode,
	                p.PrnCode,
	                p.Name,
	                p.ParentRowNum,
	                p.Nvalue,
	                p.EnteredDate,
	                p.CurrencyId,
	                p.CurrencyMeasureId,
	                p.Datum,
	                p.ValueAllowed,
	                p.PlannedYear,
	                p.IsActive,
	                p.OrderNum,
	                p.ApprovedValue,
	                p.CalculatedValue,
                    p.IsCalculated,
                    a.Name as FunctionalSubAreaName,
                    c.Name as CurrencyName,
                    m.Name as CurrencyMeasureName,
                    s.Name as CourtName

             from ProgramDataCourt p
             left join FunctionalSubArea a on p.FunctionalSubAreaId=a.Id
             left join Currency c on p.CurrencyId=c.Id
             left join CurrencyMeasure m on p.CurrencyMeasureId=m.Id
             left join Court s on p.CourtId=s.Id
             where p.ProgramDefNum={programDefNum??0} and p.PlannedYear={ny??0} and p.RowNum={rowNum ?? 0}";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDataCourtGridVm>(sql2);
            return result?.ToList();
        }
        public async Task<IEnumerable<ProgramDataCourtGridVm>> GetProgramDataCourtGridByCourtIdsAsync(int? programDefNum, int? ny, IEnumerable<int> Ids)
        {
            string sql2 = $@"select p.Id,
                    p.CourtId,
	                p.ProgramDefNum,
	                p.FunctionalAreaId,
	                p.FunctionalSubAreaId,
	                p.FunctionalActionId,
	                p.RowNum,
	                p.RowCode,
	                p.PrnCode,
	                p.Name,
	                p.ParentRowNum,
	                p.Nvalue,
	                p.EnteredDate,
	                p.CurrencyId,
	                p.CurrencyMeasureId,
	                p.Datum,
	                p.ValueAllowed,
	                p.PlannedYear,
	                p.IsActive,
	                p.OrderNum,
	                p.ApprovedValue,
	                p.CalculatedValue,
                    p.IsCalculated,
                    a.Name as FunctionalSubAreaName,
                    c.Name as CurrencyName,
                    m.Name as CurrencyMeasureName,
                    s.Name as CourtName

             from ProgramDataCourt p
             left join FunctionalSubArea a on p.FunctionalSubAreaId=a.Id
             left join Currency c on p.CurrencyId=c.Id
             left join CurrencyMeasure m on p.CurrencyMeasureId=m.Id
             left join Court s on p.CourtId=s.Id
             where p.ProgramDefNum={programDefNum ?? 0} and p.PlannedYear={ny ?? 0} and p.courtId in({string.Join<int>(",", Ids)})";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDataCourtGridVm>(sql2);
            return result?.ToList();
        }
        public async Task<IEnumerable<ProgramDataCourtGridVm>> GetProgramDataCourtGridByCourtIdAsync(int? programDefNum, int? ny,  int? courtId) {
            string sql2 = $@"select p.Id,
                    p.CourtId,
	                p.ProgramDefNum,
	                p.FunctionalAreaId,
	                p.FunctionalSubAreaId,
	                p.FunctionalActionId,
	                p.RowNum,
	                p.RowCode,
	                p.PrnCode,
	                p.Name,
	                p.ParentRowNum,
	                p.Nvalue,
	                p.EnteredDate,
	                p.CurrencyId,
	                p.CurrencyMeasureId,
	                p.Datum,
	                p.ValueAllowed,
	                p.PlannedYear,
	                p.IsActive,
	                p.OrderNum,
	                p.ApprovedValue,
	                p.CalculatedValue,
                    p.IsCalculated, 
                    a.Name as FunctionalSubAreaName,
                    c.Name as CurrencyName,
                    m.Name as CurrencyMeasureName,
                    s.Name as CourtName

             from ProgramDataCourt p
             left join FunctionalSubArea a on p.FunctionalSubAreaId=a.Id
             left join Currency c on p.CurrencyId=c.Id
             left join CurrencyMeasure m on p.CurrencyMeasureId=m.Id
             left join Court s on p.CourtId=s.Id
             where p.ProgramDefNum={programDefNum ?? 0} and p.PlannedYear={ny ?? 0} and p.CourtId={courtId??0} and p.IsActive=1";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDataCourtGridVm>(sql2);
            return result?.ToList();
        }
        public async Task<IEnumerable<ProgramDataCourtGridVm>> GetProgramDataCourtByCourtIdAsync(int? courtId, int? ny) {
            string sql2 = $@"select p.Id,
                    p.CourtId,
	                p.ProgramDefNum,
	                p.FunctionalAreaId,
	                p.FunctionalSubAreaId,
	                p.FunctionalActionId,
	                p.RowNum,
	                p.RowCode,
	                p.PrnCode,
	                p.Name,
	                p.ParentRowNum,
	                p.Nvalue,
	                p.EnteredDate,
	                p.CurrencyId,
	                p.CurrencyMeasureId,
	                p.Datum,
	                p.ValueAllowed,
	                p.PlannedYear,
	                p.IsActive,
	                p.OrderNum,
	                p.ApprovedValue,
	                p.CalculatedValue,
                    p.IsCalculated, 
                    a.Name as FunctionalSubAreaName,
                    c.Name as CurrencyName,
                    m.Name as CurrencyMeasureName,
                    s.Name as CourtName

             from ProgramDataCourt p
             left join FunctionalSubArea a on p.FunctionalSubAreaId=a.Id
             left join Currency c on p.CurrencyId=c.Id
             left join CurrencyMeasure m on p.CurrencyMeasureId=m.Id
             left join Court s on p.CourtId=s.Id
             where p.CourtId={courtId ?? 0} and p.PlannedYear={ny ?? 0}";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDataCourtGridVm>(sql2);
            return result?.ToList();
        }
        public async Task<IEnumerable<DraftBudgetDataVm>> GetDraftBudgetDataByCourtIdAsync(int? courtId, int? ny)
        {
            string sql2 = $@"select p.Id
                      ,p.CourtId
                      ,p.FunctionalSubAreaId
                      ,p.RowNum
                      ,p.NYear
                      ,p.Nvalue
                      ,p.CurrencyId
                      ,p.Par
                      ,p.CurrencyMeasureId
                      ,p.Datum
                      ,p.Code
                      ,p.RowCode
                      ,p.ParentRowNum
                    ,a.Name as FunctionalSubAreaName,
                    c.Name as CurrencyName,
                    m.Name as CurrencyMeasureName,
                    s.Name as CourtName

             from DraftBudgetData p
             left join FunctionalSubArea a on p.FunctionalSubAreaId=a.Id
             left join Currency c on p.CurrencyId=c.Id
             left join CurrencyMeasure m on p.CurrencyMeasureId=m.Id
             left join Court s on p.CourtId=s.Id
             where p.CourtId={courtId ?? 0} and p.NYear={ny ?? 0}";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<DraftBudgetDataVm>(sql2);
            return result?.ToList();

        }
        public async Task<int> UpdateProgramDataValueByIdAsync(int? id, string fieldName, decimal? val) {
            var sql = $@"UPDATE ProgramData SET {fieldName} = {val}, EnteredDate=getDate() WHERE Id ={id??0}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql);

            return affectedRows.Result;
        }
        public async Task<int> UpdateProgramData3YValueByIdAsync(int? id, string fieldName, decimal? val) {
            var rec = await GetProgramDataByIdAsync(id ?? 0);
            int currentYear= rec?.PlannedYear ?? 0;
           
            switch (fieldName.ToLower()) {
                case "nval2":
                    currentYear = currentYear + 1;
                    break;
                case "nval3": currentYear = currentYear + 2; break;
            }
            var sql = $@"UPDATE ProgramData SET Nvalue = {val}, EnteredDate=getDate() WHERE FunctionalSubAreaId={rec?.FunctionalSubAreaId} and PlannedYear={currentYear} and RowNum={rec?.RowNum??0}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql);

            return affectedRows.Result;
        }
        public async Task<int> UpdateProgramDataCourt3YValueByIdAsync(int? id, string fieldName, decimal? val) {
            var rec = await GetProgramDataCourtByIdAsync(id ?? 0);
            int currentYear = rec?.PlannedYear ?? 0;

            switch (fieldName.ToLower())
            {
                case "nval2":
                    currentYear = currentYear + 1;
                    break;
                case "nval3": currentYear = currentYear + 2; break;
            }
            var sql = $@"UPDATE ProgramDataCourt SET Nvalue = {val}, EnteredDate=getDate() WHERE FunctionalSubAreaId={rec?.FunctionalSubAreaId} and PlannedYear={currentYear} and RowNum={rec?.RowNum ?? 0} and CourtId={rec.CourtId}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql);

            return affectedRows.Result;
        }
        public async Task<int> UpdateProgramDataInstitution3YValueByIdAsync(int? id, string fieldName, decimal? val)
        {
            var rec = await GetProgramDataInstitutionByIdAsync(id ?? 0);
            int currentYear = rec?.PlannedYear ?? 0;

            switch (fieldName.ToLower())
            {
                case "nval2":
                    currentYear = currentYear + 1;
                    break;
                case "nval3": currentYear = currentYear + 2; break;
            }
            var sql = $@"UPDATE ProgramDataInstitution SET Nvalue = {val}, EnteredDate=getDate() WHERE FunctionalSubAreaId={rec?.FunctionalSubAreaId} and PlannedYear={currentYear} and RowNum={rec?.RowNum ?? 0} and InstitutionTypeId={rec.InstitutionTypeId}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql);

            return affectedRows.Result;
        }
      
        public async Task<int> UpdateProgramDataCourtValueByIdAsync(int? id, string fieldName, decimal? val) {
            var sql = $@"UPDATE ProgramDataCourt SET {fieldName} = {val}, EnteredDate=getDate() WHERE Id ={id ?? 0}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql);

            return affectedRows.Result;
        }
        public async Task<IEnumerable<IdNames>> GetCurrencies() {
            string sql = $@"select Id,Name from Currency";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql);
            return result?.ToList();
        }
        public async Task<IEnumerable<IdNames>> GetCurrencyMeasures()
        {
            string sql = $@"select Id,Name from CurrencyMeasure";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql);
            return result?.ToList();
        }
        public async Task<string> GetNameByIdFromTable(string tableName, int? Id) {
            string sql = $@"select Name from {tableName} where Id={Id??0}";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql);
            return result?.FirstOrDefault()?.Name??string.Empty;
        }
        public async Task<IEnumerable<IdNames>> GetCourtsInProgramData(int? programNum) {
            string sql = $@"select a.CourtId as Id, c.Name as Name
                  from CourtInProgram a
                  left join Court c on a.CourtId=c.Id
                  where FunctionalSubAreaId={programNum??0}";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<IdNames>(sql);
            return result?.ToList();
        }
        public async Task<string> GetKontoCodesFromProgramDef(int? functionalSubAreaId, int? rowNum) {
            string sql = $@"select top 1 KontoCodes  from ProgramDef where FunctionalSubAreaId={functionalSubAreaId??0} and RowNum={rowNum??0} and KontoCodes is not null";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDefKontoCodesVm>(sql);
            return result?.FirstOrDefault()?.KontoCodes??string.Empty;
        }
        public async Task<string> GetKontoCodesFromDraftBudgetDefByParAsync(string sPar) {
            string sql = $@"select top 1 Codes  from DraftBudgetDef where Par='{sPar}'";

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<ProgramDefKontoCodesVm>(sql);
            return result?.FirstOrDefault()?.KontoCodes ?? string.Empty;
        }
        public async Task<int> CheckKontoMonthData(int? courtId, int? functionalSubAreaId, int? rowNum, int? nm, int? ny) {
            var sql = $"SELECT top 1 Id FROM KontoMonthData WHERE CourtId={courtId??0} and FunctionalSubAreaId={functionalSubAreaId??0} and RowNum={rowNum??0} and NMonth={nm??0}  and NYear={ny??0}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<int>(sql, new { CourtId = courtId, NMonth = nm, NYear = ny });
            return (result.Any())?result.FirstOrDefault():0;
        }
        public async Task<int> AddUpdateKontoMonthData(KontoMonthDataVm data) {
             int kmdId = await CheckKontoMonthData(data?.CourtId, data?.FunctionalSubAreaId, data?.RowNum, data?.NMonth, data?.NYear);
            if (kmdId==0)
            {
                var sql = $@"INSERT INTO [dbo].[KontoMonthData]
           ([CourtId]
           ,[ProgramDefId]
           ,[FunctionalSubAreaId]
           ,[RowNum]
           ,[RowCode]
           ,[NMonth]
           ,[NYear]
           ,[Nvalue]
           ,[CurrencyId]
           ,[CurrencyMeasureId])
     VALUES
           ({data?.CourtId??0}
           ,{data?.ProgramDefId ?? 0}
           ,{data?.FunctionalSubAreaId ?? 0}
           ,{data?.RowNum??0}
           ,'{data?.RowCode ?? string.Empty}'
           ,{data?.NMonth??0} 
           ,{data?.NYear??0}
           ,{data?.Nvalue ?? 0}
           ,{data?.CurrencyId ?? 0}
           ,{data?.CurrencyMeasureId ?? 0})";
                await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
                await connection.OpenAsync();
                var affectedRows = connection.ExecuteAsync(sql);

                return affectedRows.Result;
            }
            else {
                var sql = $@"UPDATE KontoMonthData SET Nvalue = {data?.Nvalue} WHERE Id = {kmdId}";
                await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
                await connection.OpenAsync();
                var affectedRows = connection.ExecuteAsync(sql);

                return affectedRows.Result;
            }
        }
        private async Task<int> CheckDraftBudgetYearData(int? courtId, int? functionalSubAreaId, int? rowNum,  int? ny)
        {
            var sql = $"SELECT top 1 Id FROM DraftBudgetData WHERE CourtId={courtId ?? 0} and FunctionalSubAreaId={functionalSubAreaId ?? 0} and RowNum={rowNum ?? 0}  and NYear={ny ?? 0}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<int>(sql);
            return (result.Any()) ? result.FirstOrDefault() : 0;
        }
        public async Task<int> AddUpdateDraftBudgetYearDataAsync(DraftBudgetDataVm data) {
            int kmdId = await CheckDraftBudgetYearData(data?.CourtId, data?.FunctionalSubAreaId, data?.RowNum, data?.NYear);
            if (kmdId == 0)
            {
                var sql = $@"INSERT INTO [dbo].[DraftBudgetData]
           ([CourtId]
           ,[FunctionalSubAreaId]
           ,[RowNum]
           ,[NYear]
           ,[Nvalue]
           ,[CurrencyId]
           ,[Par]
           ,[CurrencyMeasureId])
     VALUES
           ({data?.CourtId ?? 0}
           ,{data?.FunctionalSubAreaId ?? 0}
           ,{data?.RowNum ?? 0}
           ,{data?.NYear ?? 0}
           ,{data?.Nvalue ?? 0}
           ,{data?.CurrencyId ?? 0}
           ,{data?.Par ?? string.Empty}
           ,{data?.CurrencyMeasureId ?? 0})";
                await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
                await connection.OpenAsync();
                var affectedRows = connection.ExecuteAsync(sql);

                return affectedRows.Result;
            }
            else
            {
                var sql = $@"UPDATE DraftBudgetData SET Nvalue = {data?.Nvalue} WHERE Id = {kmdId}";
                await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
                await connection.OpenAsync();
                var affectedRows = connection.ExecuteAsync(sql);

                return affectedRows.Result;
            }
        }
        private async Task<decimal?> GetSumCalcValuesByCourtIdYearAsync(int? courtId, int? functionalSubAreaId, int? rowNum, int? ny) {
            var sql = $@"Select Sum(Nvalue) from KontoMonthData where courtid={courtId ?? 0} and FunctionalSubAreaId={functionalSubAreaId ?? 0} and rowNum={rowNum ?? 0} and Nyear={ny??0}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var nSum = connection.ExecuteScalarAsync<decimal>(sql);

            return (nSum!=null)?nSum.Result:0;
        }
        public async Task<int> ProgramDataCourtAsync(int? courtId,  int? functionalSubAreaId, int? rowNum, int? nYear) {
            decimal? calculatedVal = await GetSumCalcValuesByCourtIdYearAsync(courtId,functionalSubAreaId,  rowNum, nYear);
            var sql = $@"Update ProgramDataCourt set CalculatedValue={calculatedVal??0} where courtid={courtId??0} and FunctionalSubAreaId={functionalSubAreaId??0} and rowNum={rowNum??0}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql);

            return affectedRows.Result;
        }
        public async Task<int> ProgramDataDraftBudgetCourtAsync(int? courtId, int? functionalSubAreaId, int? rowNum, int? nYear, decimal? nValue) {
            var sql = $@"Update ProgramDataCourt set NValue={nValue ?? 0} where courtid={courtId ?? 0} and FunctionalSubAreaId={functionalSubAreaId ?? 0} and PlannedYear={nYear ?? 0} and rowNum={rowNum ?? 0} ";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql);

            return affectedRows.Result;
        }
        public async Task<int> ProgramDataDraftBudgetInstitutionAsync(int? institutionTypeId, int? functionalSubAreaId, int? rowNum, int? nYear, decimal? nValue)
        {
            var sql = $@"Update ProgramDataInstitution set NValue={nValue ?? 0} where InstitutionTypeId={institutionTypeId ?? 0} and FunctionalSubAreaId={functionalSubAreaId ?? 0} and PlannedYear={nYear ?? 0} and rowNum={rowNum ?? 0} ";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql);

            return affectedRows.Result;
        }
        public async Task<int> FirstInitProgramDataDraftBudgetCourtAsync(int? courtId, int? functionalSubAreaId, int? nYear) {
            var sql = $@"Update ProgramDataCourt set NValue=0 where courtid={courtId ?? 0} and FunctionalSubAreaId={functionalSubAreaId ?? 0} and PlannedYear={nYear ?? 0} ";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql);

            return affectedRows.Result;
        }
        public async Task<int> FirstInitProgramDataDraftBudgetInstitutionAsync(int? institutionTypeId, int? functionalSubAreaId, int? nYear)
        {
            var sql = $@"Update ProgramDataInstitution set NValue=0 where InstitutionTypeId={institutionTypeId ?? 0} and FunctionalSubAreaId={functionalSubAreaId ?? 0} and PlannedYear={nYear ?? 0} ";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql);

            return affectedRows.Result;
        }
        public async Task<IEnumerable<KontoCourtsYearVm>> GetKontoCourtsYearAsync(int? institutionTypeId, int? courtTypeId,int? courtId, int? ny, int? nmonth, int? reportTypeId) {
            string sql = string.Empty;

            if (reportTypeId == 1)
            {
                sql = $@"select ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) as id, {nmonth} as Nmonth, k.NYear,Sum(k.Nvalue) as Nvalue,c.Name as CourtName,f.Name as ProgramName, p.NAme as RowName
                                      from    KontoMonthData k 
									   left join Court c on k.CourtId=c.Id
                          left join FunctionalSubArea f on k.FunctionalSubAreaId=f.id
                          left join ProgramDef p on k.FunctionalSubAreaId=p.FunctionalSubAreaId and k.RowNum=p.RowNum
									  where k.NYear={ny ?? 0} and k.NMonth<={nmonth ?? 0} and c.courtTypeId in( select distinct Id from courtType where InstitutionTypeId={institutionTypeId??0}) ";


                if (courtId > 0)
                {
                    sql += $" and k.CourtId={courtId ?? 0} ";
                }
                sql += " group by  k.CourtId,k.FunctionalSubAreaId,k.RowNum,k.NYear,c.Name,f.Name,p.Name ";


            }
            else if (reportTypeId == 2) {
                sql = $@"select k.Id,k.Nmonth,k.Nyear,k.Nvalue,c.Name as CourtName,f.Name as ProgramName, p.NAme as RowName
                          from KontoMonthData k
                          left join Court c on k.CourtId=c.Id
                          left join FunctionalSubArea f on k.FunctionalSubAreaId=f.id
                          left join ProgramDef p on k.FunctionalSubAreaId=p.FunctionalSubAreaId and k.RowNum=p.RowNum
                          where k.NYear={ny ?? 0} and c.courtTypeId in( select distinct Id from courtType where InstitutionTypeId={institutionTypeId ?? 0})";
                if (courtId > 0)
                {
                    sql += $" and k.CourtId={courtId ?? 0} ";
                }
                
            }
            else
            {
                sql = $@"select k.Id,k.Nmonth,k.Nyear,k.Nvalue,c.Name as CourtName,f.Name as ProgramName, p.NAme as RowName
                          from KontoMonthData k
                          left join Court c on k.CourtId=c.Id
                          left join FunctionalSubArea f on k.FunctionalSubAreaId=f.id
                          left join ProgramDef p on k.FunctionalSubAreaId=p.FunctionalSubAreaId and k.RowNum=p.RowNum
                          where k.NYear={ny ?? 0} and c.courtTypeId in( select distinct Id from courtType where InstitutionTypeId={institutionTypeId ?? 0}) ";
                if (courtId > 0)
                {
                    sql += $" and k.CourtId={courtId ?? 0} ";
                }
                if (nmonth > 0)
                {
                    sql += $" and k.Nmonth={nmonth ?? 0} ";
                }
            }


          

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<KontoCourtsYearVm>(sql);
            return result?.ToList();
        }
        public async Task<int> CalculateProgramDataValues(int functionalSubAreaId, int rowNum, int plannedYear, decimal val) {
            var sql = @"UPDATE ProgramData SET Nvalue = @Nvalue WHERE functionalSubAreaId = @FunctionalSubAreaId and plannedYear=@PlannedYear and RowNum=@RowNum";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var affectedRows = connection.ExecuteAsync(sql, new { FunctionalSubAreaId=functionalSubAreaId,RowNum=rowNum,PlannedYear=plannedYear, Nvalue = val});

            return affectedRows.Result;
        }
        public async Task<int?> sp_RecalculateProgramDataAsync(int? functionalSubAreaId, int? ny) {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("functionalSubAreaId", functionalSubAreaId ?? 0);
            parameters.Add("nYear", ny);
            var ret = connection.ExecuteAsync("sp_RecalculateProgramData", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }
        public async Task<int?> sp_RecalculateProgramDataCourtAsync(int? functionalSubAreaId, int? ny, int? courtId) {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("functionalSubAreaId", functionalSubAreaId ?? 0);
            parameters.Add("nYear", ny);
            parameters.Add("CourtId", courtId??0);
            var ret = connection.ExecuteAsync("sp_RecalculateProgramDataCourt", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }
        public async Task<int?> sp_RecalculateProgramDataInstitutionAsync(int? functionalSubAreaId, int? ny, int? institutionTypeId)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("functionalSubAreaId", functionalSubAreaId ?? 0);
            parameters.Add("nYear", ny);
            parameters.Add("InstitutionTypeId", institutionTypeId ?? 0);
            var ret = connection.ExecuteAsync("sp_RecalculateProgramDataInstitution", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }
        public async Task<int?> sp_UpdateProgramsByProgramDefAsync(int? Id)
        {
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Id", Id ?? 0);
            var ret = connection.ExecuteAsync("sp_UpdateProgramsByProgramDef", parameters, commandType: CommandType.StoredProcedure);
            return ret.Result;
        }
        public async Task<IEnumerable<string>> GetCourtNamesByIds(IEnumerable<int> ids) {
            string sql = string.Empty;
            
                sql = $@"select Name from Court where id in({string.Join<int>(",", ids)})";
            

            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.QueryAsync<string>(sql);
            return result?.ToList();
        }
        private async Task<decimal> GetValueFromMainDataItems(int m1, int m2, int ny, int courtId, int matricsFieldId) {
            var sql = $@"Select Sum(Nvalue) from MainDataItems where NMonth>={m1} and NMonth<={m2} and NYear={ny} and CourtId={courtId} and  MetricsFieldId={matricsFieldId}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var nSum = connection.ExecuteScalarAsync<decimal>(sql);

            return (nSum != null) ? nSum.Result : 0;
        }
        private async Task<decimal> GetValueFromMainDataItemsByCourtIds(int m1, int m2, int ny, IEnumerable<int> courtIds, int matricsFieldId)
        {
            var sql = $@"Select Sum(Nvalue) from MainDataItems where NMonth>={m1} and NMonth<={m2} and NYear={ny} and CourtId in({string.Join<int>(",", courtIds)}) and  MetricsFieldId={matricsFieldId}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var nSum = connection.ExecuteScalarAsync<decimal>(sql);

            return (nSum != null) ? nSum.Result : 0;
        }
        private async Task<decimal> GetValueFromMainDataCommonItems(int m1, int m2, int ny,  int matricsFieldId)
        {
            var sql = $@"Select Sum(Nvalue) from MainDataItems where NMonth>={m1} and NMonth<={m2} and NYear={ny}  and  MetricsFieldId={matricsFieldId}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var nSum = connection.ExecuteScalarAsync<decimal>(sql);

            return (nSum != null) ? nSum.Result : 0;
        }
        private async Task<decimal> GetKontoMonthDataValue(int functionalSubAreaId, int rowNum, int m1, int m2, int ny) {
            var sql = $@"Select Sum(Nvalue) from KontoMonthData where FunctionalSubAreaId={functionalSubAreaId} and [RowNum]={rowNum} and NMonth>={m1} and NMonth<={m2} and NYear={ny}";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var nSum = connection.ExecuteScalarAsync<decimal>(sql);

            return (nSum != null) ? nSum.Result : 0;
        }
        private async Task<decimal> GetKontoMonthDataByCourtIdsValue(int functionalSubAreaId, int rowNum, int m1, int m2, int ny,IEnumerable<int> courtIds)
        {
            var sql = $@"Select Sum(Nvalue) from KontoMonthData where FunctionalSubAreaId={functionalSubAreaId} and [RowNum]={rowNum} and NMonth>={m1} and NMonth<={m2} and NYear={ny} and CourtId in({string.Join<int>(",", courtIds)})";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var nSum = connection.ExecuteScalarAsync<decimal>(sql);

            return (nSum != null) ? nSum.Result : 0;
        }

        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataProgram1Async(int m1, int m2, int nyear) { 
            var ret=new List<ProgramDataExecutionVm>();
            if(m1==0||m2==0||nyear==0) return new List<ProgramDataExecutionVm>();
            int nMonthNum=m2-m1;
            if(nMonthNum<1) nMonthNum=1;
            decimal nMagReal = await GetValueFromMainDataItems(m1,m2,nyear,255,63);
            nMagReal=Math.Round(nMagReal/nMonthNum,2,MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 255, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 255, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 255, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataGridByFilterAsync(1, nyear);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any()) { 
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(1, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11) { 
                        nVal=Math.Round((nCalculatedValue-p7r6),2,MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0)) { 
                            nVal=Math.Round((nVal/nCourtEmplReal)*nMagReal,2,MidpointRounding.AwayFromZero) ;
                        }
                    }
                     if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);
                        
                    }
                    if (item.RowNum==32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId=item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed=item.ValueAllowed??false;
                    ret.Add(rec);
                }
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault()??0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault()??0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault()??0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault()??0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault()??0; }
               
                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault()??0+ ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault()??0+ ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault()??0;  }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r17 = ret.Where(x => x.RowNum == 17).FirstOrDefault();
                if (r17 != null) { r17.Nvalue = ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ??0; }

                var r30 = ret.Where(x => x.RowNum == 30).FirstOrDefault();
                if (r30 != null) { r30.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ??0; }

                var r31 = ret.Where(x => x.RowNum == 31).FirstOrDefault();
                if (r31 != null) { r31.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ??0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataProgram2Async(int m1, int m2, int nyear)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 257, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 257, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 257, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 257, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataGridByFilterAsync(2, nyear);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(2, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }
                //var z = ret;
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault()??0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault()??0;  }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault()??0; }



                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue =   ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0;  }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                               
                var r18 = ret.Where(x => x.RowNum == 18).FirstOrDefault();
                if (r18 != null) { r18.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r19 = ret.Where(x => x.RowNum == 19).FirstOrDefault();
                if (r19 != null) { r19.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataProgram3Async(int m1, int m2, int nyear)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 179, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 179, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 179, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 179, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataGridByFilterAsync(3, nyear);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(3, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault()??0+ ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault()??0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault()??0; }

             

                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = (ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault())??0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = (ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault()+ ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault()+ ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault())??0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r17 = ret.Where(x => x.RowNum == 17).FirstOrDefault();
                if (r17 != null) { r17.Nvalue = ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault()??0+ ret.Where(x => x.RowNum == 19).Select(x => x.Nvalue).FirstOrDefault()??0+ ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r22 = ret.Where(x => x.RowNum == 22).FirstOrDefault();
                if (r22 != null) { r22.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r23 = ret.Where(x => x.RowNum == 23).FirstOrDefault();
                if (r23 != null) { r23.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault()??0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataProgram4Async(int m1, int m2, int nyear)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 180, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 180, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 180, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 180, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataGridByFilterAsync(4, nyear);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(4, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault()??0+ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault()??0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 19).Select(x => x.Nvalue).FirstOrDefault()??0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue =ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r17 = ret.Where(x => x.RowNum == 17).FirstOrDefault();
                if (r17 != null) { r17.Nvalue = ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 19).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r22 = ret.Where(x => x.RowNum == 22).FirstOrDefault();
                if (r22 != null) { r22.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r23 = ret.Where(x => x.RowNum == 23).FirstOrDefault();
                if (r23 != null) { r23.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault()??0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataProgram5Async(int m1, int m2, int nyear)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataCommonItems(m1, m2, nyear, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataCommonItems(m1, m2, nyear, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataCommonItems(m1, m2, nyear, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataCommonItems(m1, m2, nyear, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataGridByFilterAsync(5, nyear);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(5, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault()??0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault()??0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault()??0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault()??0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault()??0+ ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault()??0+ ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r18 = ret.Where(x => x.RowNum == 18).FirstOrDefault();
                if (r18 != null) { r18.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r19 = ret.Where(x => x.RowNum == 19).FirstOrDefault();
                if (r19 != null) { r19.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault()??0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataProgram6Async(int m1, int m2, int nyear)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 254, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 254, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 254, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 254, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataGridByFilterAsync(6, nyear);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(6, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault()??0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 19).Select(x => x.Nvalue).FirstOrDefault()??0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault()??0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault()??0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r17 = ret.Where(x => x.RowNum == 17).FirstOrDefault();
                if (r17 != null) { r17.Nvalue = ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 19).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r22 = ret.Where(x => x.RowNum == 22).FirstOrDefault();
                if (r22 != null) { r22.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r23 = ret.Where(x => x.RowNum == 23).FirstOrDefault();
                if (r23 != null) { r23.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault()??0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataProgram7Async(int m1, int m2, int nyear)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
           
            var programData = await GetProgramDataGridByFilterAsync(7, nyear);
            decimal nCalculatedValue = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(7, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal=nCalculatedValue;
                   
                  
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault()??0;  }

                               
                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r22 = ret.Where(x => x.RowNum == 22).FirstOrDefault();
                if (r22 != null) { r22.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r23 = ret.Where(x => x.RowNum == 23).FirstOrDefault();
                if (r23 != null) { r23.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault()??0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataProgram8Async(int m1, int m2, int nyear)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 256, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 256, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 256, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 256, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataGridByFilterAsync(8, nyear);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(8, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                  
                    if (item.RowNum == 7)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 8)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }
               

                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault()??0; }
                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r22 = ret.Where(x => x.RowNum == 22).FirstOrDefault();
                if (r22 != null) { r22.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r23 = ret.Where(x => x.RowNum == 23).FirstOrDefault();
                if (r23 != null) { r23.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault()??0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataProgram9Async(int m1, int m2, int nyear)
        {
            var ret = new List<ProgramDataExecutionVm>();
           
            var programData = await GetProgramDataGridByFilterAsync(9, nyear);
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nVal = await GetKontoMonthDataValue(9, item?.RowNum ?? 0, m1, m2, nyear);
                   


                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 5).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault()??0+ ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault()??0; }


                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault()??0+ ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault()??0; }

                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault()??0 + ret.Where(x => x.RowNum == 9).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault()??0; }


            }
            return ret;
        }
        public async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataGridAsync(int? functionalSubAreaId, int? m1, int? m2, int? nyear) {
            switch (functionalSubAreaId) {
                case 1:return await GetYearExecutionDataProgram1Async(m1??0, m2??0, nyear ?? 0);
                case 2: return await GetYearExecutionDataProgram2Async(m1 ?? 0, m2 ?? 0, nyear ?? 0);
                case 3: return await GetYearExecutionDataProgram3Async(m1 ?? 0, m2 ?? 0, nyear ?? 0);
                case 4: return await GetYearExecutionDataProgram4Async(m1 ?? 0, m2 ?? 0, nyear ?? 0);
                case 5: return await GetYearExecutionDataProgram5Async(m1 ?? 0, m2 ?? 0, nyear ?? 0);
                case 6: return await GetYearExecutionDataProgram6Async(m1 ?? 0, m2 ?? 0, nyear ?? 0);
                case 7: return await GetYearExecutionDataProgram7Async(m1 ?? 0, m2 ?? 0, nyear ?? 0);
                case 8: return await GetYearExecutionDataProgram8Async(m1 ?? 0, m2 ?? 0, nyear ?? 0);
                case 9: return await GetYearExecutionDataProgram9Async(m1 ?? 0, m2 ?? 0, nyear ?? 0);
                default:return new List<ProgramDataExecutionVm>();
            }
        }

        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtRowProgram1Async(int m1, int m2, int nyear, int rowNum)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 255, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 255, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 255, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 255, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataCourtGridByFilterAsync(1,  nyear, rowNum);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(1, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    rec.CourtName = item.CourtName ?? string.Empty;
                    ret.Add(rec);
                }
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r17 = ret.Where(x => x.RowNum == 17).FirstOrDefault();
                if (r17 != null) { r17.Nvalue = ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r30 = ret.Where(x => x.RowNum == 30).FirstOrDefault();
                if (r30 != null) { r30.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r31 = ret.Where(x => x.RowNum == 31).FirstOrDefault();
                if (r31 != null) { r31.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtRowProgram2Async(int m1, int m2, int nyear, int rowNum)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 257, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 257, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 257, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 257, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataCourtGridByFilterAsync(2, nyear, rowNum);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(2, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    rec.CourtName = item.CourtName ?? string.Empty;
                    ret.Add(rec);
                }
                //var z = ret;
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }



                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r18 = ret.Where(x => x.RowNum == 18).FirstOrDefault();
                if (r18 != null) { r18.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r19 = ret.Where(x => x.RowNum == 19).FirstOrDefault();
                if (r19 != null) { r19.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtRowProgram3Async(int m1, int m2, int nyear, int rowNum)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 179, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 179, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 179, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 179, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataCourtGridByFilterAsync(3, nyear, rowNum);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(3, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    rec.CourtName = item.CourtName ?? string.Empty;
                    ret.Add(rec);
                }
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }



                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = (ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault()) ?? 0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = (ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault()) ?? 0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r17 = ret.Where(x => x.RowNum == 17).FirstOrDefault();
                if (r17 != null) { r17.Nvalue = ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 19).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r22 = ret.Where(x => x.RowNum == 22).FirstOrDefault();
                if (r22 != null) { r22.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r23 = ret.Where(x => x.RowNum == 23).FirstOrDefault();
                if (r23 != null) { r23.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtRowProgram4Async(int m1, int m2, int nyear, int rowNum)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 180, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 180, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 180, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 180, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataCourtGridByFilterAsync(4, nyear, rowNum);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(4, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    rec.CourtName = item.CourtName ?? string.Empty;
                    ret.Add(rec);
                }
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 19).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r17 = ret.Where(x => x.RowNum == 17).FirstOrDefault();
                if (r17 != null) { r17.Nvalue = ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 19).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r22 = ret.Where(x => x.RowNum == 22).FirstOrDefault();
                if (r22 != null) { r22.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r23 = ret.Where(x => x.RowNum == 23).FirstOrDefault();
                if (r23 != null) { r23.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtRowProgram5Async(int m1, int m2, int nyear, int rowNum)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataCommonItems(m1, m2, nyear, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataCommonItems(m1, m2, nyear, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataCommonItems(m1, m2, nyear, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataCommonItems(m1, m2, nyear, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataCourtGridByFilterAsync(5, nyear, rowNum);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(5, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    rec.CourtName = item.CourtName ?? string.Empty;
                    ret.Add(rec);
                }
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r18 = ret.Where(x => x.RowNum == 18).FirstOrDefault();
                if (r18 != null) { r18.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r19 = ret.Where(x => x.RowNum == 19).FirstOrDefault();
                if (r19 != null) { r19.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtRowProgram6Async(int m1, int m2, int nyear, int rowNum)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 254, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 254, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 254, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 254, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataCourtGridByFilterAsync(6, nyear, rowNum);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(6, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    rec.CourtName = item.CourtName ?? string.Empty;
                    ret.Add(rec);
                }
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 19).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r17 = ret.Where(x => x.RowNum == 17).FirstOrDefault();
                if (r17 != null) { r17.Nvalue = ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 19).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r22 = ret.Where(x => x.RowNum == 22).FirstOrDefault();
                if (r22 != null) { r22.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r23 = ret.Where(x => x.RowNum == 23).FirstOrDefault();
                if (r23 != null) { r23.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtRowProgram7Async(int m1, int m2, int nyear, int rowNum)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();

            var programData = await GetProgramDataCourtGridByFilterAsync(7, nyear, rowNum);
            decimal nCalculatedValue = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(7, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal = nCalculatedValue;


                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    rec.CourtName = item.CourtName ?? string.Empty;
                    ret.Add(rec);
                }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r22 = ret.Where(x => x.RowNum == 22).FirstOrDefault();
                if (r22 != null) { r22.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r23 = ret.Where(x => x.RowNum == 23).FirstOrDefault();
                if (r23 != null) { r23.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtRowProgram8Async(int m1, int m2, int nyear, int rowNum)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 256, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 256, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 256, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 256, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataCourtGridByFilterAsync(8, nyear, rowNum);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataValue(8, item?.RowNum ?? 0, m1, m2, nyear);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);


                    if (item.RowNum == 7)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 8)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    rec.CourtName = item.CourtName ?? string.Empty;
                    ret.Add(rec);
                }


                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r22 = ret.Where(x => x.RowNum == 22).FirstOrDefault();
                if (r22 != null) { r22.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r23 = ret.Where(x => x.RowNum == 23).FirstOrDefault();
                if (r23 != null) { r23.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtRowProgram9Async(int m1, int m2, int nyear, int rowNum)
        {
            var ret = new List<ProgramDataExecutionVm>();

            var programData = await GetProgramDataCourtGridByFilterAsync(9, nyear, rowNum);
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nVal = await GetKontoMonthDataValue(9, item?.RowNum ?? 0, m1, m2, nyear);



                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    rec.CourtName = item?.CourtName ?? string.Empty;
                    ret.Add(rec);
                }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 5).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 9).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        public async Task<IEnumerable<ProgramDataExecutionVm>> GetProgramDataCourtGridByFilterAsync(int? functionalSubAreaId, int? m1, int? m2, int? nyear, int? rowNum) {
            switch (functionalSubAreaId)
            {
                case 1: return await GetYearExecutionDataCourtRowProgram1Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, rowNum ?? 0);
                case 2: return await GetYearExecutionDataCourtRowProgram2Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, rowNum ?? 0);
                case 3: return await GetYearExecutionDataCourtRowProgram3Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, rowNum ?? 0);
                case 4: return await GetYearExecutionDataCourtRowProgram4Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, rowNum ?? 0);
                case 5: return await GetYearExecutionDataCourtRowProgram5Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, rowNum ?? 0);
                case 6: return await GetYearExecutionDataCourtRowProgram6Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, rowNum ?? 0);
                case 7: return await GetYearExecutionDataCourtRowProgram7Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, rowNum ?? 0);
                case 8: return await GetYearExecutionDataCourtRowProgram8Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, rowNum ?? 0);
                case 9: return await GetYearExecutionDataCourtRowProgram9Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, rowNum ?? 0);
                default: return new List<ProgramDataExecutionVm>();
            }
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtProgram1Async(int m1, int m2, int nyear, IEnumerable<int> courtIds)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 255, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 255, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 255, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 255, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataGridByFilterAsync(1, nyear);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataByCourtIdsValue(1, item?.RowNum ?? 0, m1, m2, nyear,courtIds);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r17 = ret.Where(x => x.RowNum == 17).FirstOrDefault();
                if (r17 != null) { r17.Nvalue = ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r30 = ret.Where(x => x.RowNum == 30).FirstOrDefault();
                if (r30 != null) { r30.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r31 = ret.Where(x => x.RowNum == 31).FirstOrDefault();
                if (r31 != null) { r31.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtProgram2Async(int m1, int m2, int nyear, IEnumerable<int> courtIds)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 257, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 257, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 257, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 257, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataGridByFilterAsync(2, nyear);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataByCourtIdsValue(2, item?.RowNum ?? 0, m1, m2, nyear,courtIds);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }
                //var z = ret;
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }



                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r18 = ret.Where(x => x.RowNum == 18).FirstOrDefault();
                if (r18 != null) { r18.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r19 = ret.Where(x => x.RowNum == 19).FirstOrDefault();
                if (r19 != null) { r19.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtProgram3Async(int m1, int m2, int nyear, IEnumerable<int> courtIds)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 179, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 179, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 179, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 179, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataGridByFilterAsync(3, nyear);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataByCourtIdsValue(3, item?.RowNum ?? 0, m1, m2, nyear,courtIds);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }



                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = (ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault()) ?? 0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = (ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault()) ?? 0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r17 = ret.Where(x => x.RowNum == 17).FirstOrDefault();
                if (r17 != null) { r17.Nvalue = ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 19).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r22 = ret.Where(x => x.RowNum == 22).FirstOrDefault();
                if (r22 != null) { r22.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r23 = ret.Where(x => x.RowNum == 23).FirstOrDefault();
                if (r23 != null) { r23.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtProgram4Async(int m1, int m2, int nyear, IEnumerable<int> courtIds)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 180, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 180, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 180, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 180, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await  GetProgramDataGridByFilterAsync(4, nyear);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataByCourtIdsValue(4, item?.RowNum ?? 0, m1, m2, nyear,courtIds);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 19).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r17 = ret.Where(x => x.RowNum == 17).FirstOrDefault();
                if (r17 != null) { r17.Nvalue = ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 19).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r22 = ret.Where(x => x.RowNum == 22).FirstOrDefault();
                if (r22 != null) { r22.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r23 = ret.Where(x => x.RowNum == 23).FirstOrDefault();
                if (r23 != null) { r23.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtProgram5Async(int m1, int m2, int nyear, IEnumerable<int> courtIds)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItemsByCourtIds(m1, m2, nyear,courtIds, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItemsByCourtIds(m1, m2, nyear, courtIds, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItemsByCourtIds(m1, m2, nyear, courtIds, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItemsByCourtIds(m1, m2, nyear, courtIds, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataGridByFilterAsync(5, nyear);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataByCourtIdsValue(5, item?.RowNum ?? 0, m1, m2, nyear,courtIds);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r18 = ret.Where(x => x.RowNum == 18).FirstOrDefault();
                if (r18 != null) { r18.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r19 = ret.Where(x => x.RowNum == 19).FirstOrDefault();
                if (r19 != null) { r19.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtProgram6Async(int m1, int m2, int nyear, IEnumerable<int> courtIds)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 254, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 254, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 254, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 254, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataGridByFilterAsync(6, nyear);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataByCourtIdsValue(6, item?.RowNum ?? 0, m1, m2, nyear,courtIds);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);

                    if (item.RowNum == 11)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nMagReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 14)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 18)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }
                var r6 = ret.Where(x => x.RowNum == 6).FirstOrDefault();
                if (r6 != null) { r6.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r7 = ret.Where(x => x.RowNum == 7).FirstOrDefault();
                if (r7 != null) { r7.Nvalue = ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 19).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r8 = ret.Where(x => x.RowNum == 8).FirstOrDefault();
                if (r8 != null) { r8.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 14).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r15 = ret.Where(x => x.RowNum == 15).FirstOrDefault();
                if (r15 != null) { r15.Nvalue = ret.Where(x => x.RowNum == 16).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r17 = ret.Where(x => x.RowNum == 17).FirstOrDefault();
                if (r17 != null) { r17.Nvalue = ret.Where(x => x.RowNum == 18).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 19).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 20).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r22 = ret.Where(x => x.RowNum == 22).FirstOrDefault();
                if (r22 != null) { r22.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r23 = ret.Where(x => x.RowNum == 23).FirstOrDefault();
                if (r23 != null) { r23.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtProgram7Async(int m1, int m2, int nyear, IEnumerable<int> courtIds)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();

            var programData = await GetProgramDataGridByFilterAsync(7, nyear);
            decimal nCalculatedValue = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataByCourtIdsValue(7, item?.RowNum ?? 0, m1, m2, nyear,courtIds);
                    nVal = nCalculatedValue;


                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }

                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r22 = ret.Where(x => x.RowNum == 22).FirstOrDefault();
                if (r22 != null) { r22.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r23 = ret.Where(x => x.RowNum == 23).FirstOrDefault();
                if (r23 != null) { r23.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtProgram8Async(int m1, int m2, int nyear, IEnumerable<int> courtIds)
        {
            var ret = new List<ProgramDataExecutionVm>();
            if (m1 == 0 || m2 == 0 || nyear == 0) return new List<ProgramDataExecutionVm>();
            int nMonthNum = m2 - m1;
            if (nMonthNum < 1) nMonthNum = 1;
            decimal nMagReal = await GetValueFromMainDataItems(m1, m2, nyear, 256, 63);
            nMagReal = Math.Round(nMagReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmplReal = await GetValueFromMainDataItems(m1, m2, nyear, 256, 64);
            nEmplReal = Math.Round(nEmplReal / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmplReal = nMagReal + nEmplReal;

            decimal nMag = await GetValueFromMainDataItems(m1, m2, nyear, 256, 74);
            nMag = Math.Round(nMag / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nEmpl = await GetValueFromMainDataItems(m1, m2, nyear, 256, 75);
            nEmpl = Math.Round(nEmpl / nMonthNum, 2, MidpointRounding.AwayFromZero);
            decimal nCourtEmpl = nMag + nEmpl;
            var programData = await GetProgramDataGridByFilterAsync(8, nyear);
            decimal nCalculatedValue = 0;
            decimal p7r6 = 0;
            decimal p7r7 = 0;
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nCalculatedValue = await GetKontoMonthDataByCourtIdsValue(8, item?.RowNum ?? 0, m1, m2, nyear, courtIds);
                    nVal = nCalculatedValue;
                    p7r6 = await GetKontoMonthDataValue(7, 6, m1, m2, nyear);
                    p7r7 = await GetKontoMonthDataValue(7, 7, m1, m2, nyear);


                    if (item.RowNum == 7)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r6), 2, MidpointRounding.AwayFromZero);
                        if ((nMagReal > 0) && (nEmplReal > 0))
                        {
                            nVal = Math.Round((nVal / nCourtEmplReal) * nEmplReal, 2, MidpointRounding.AwayFromZero);
                        }
                    }
                    if (item.RowNum == 8)
                    {
                        nVal = Math.Round((nCalculatedValue - p7r7), 2, MidpointRounding.AwayFromZero);

                    }
                    if (item.RowNum == 32)
                    {
                        nVal = nCourtEmpl;
                    }
                    if (item.RowNum == 33)
                    {
                        nVal = nMag;
                    }
                    if (item.RowNum == 34)
                    {
                        nVal = nEmpl;
                    }
                    if (item.RowNum == 35)
                    {
                        nVal = nCourtEmplReal;
                    }
                    if (item.RowNum == 36)
                    {
                        nVal = nMagReal;
                    }
                    if (item.RowNum == 37)
                    {
                        nVal = nEmplReal;
                    }
                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }


                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r3 = ret.Where(x => x.RowNum == 3).FirstOrDefault();
                if (r3 != null) { r3.Nvalue = ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }
                var r5 = ret.Where(x => x.RowNum == 5).FirstOrDefault();
                if (r5 != null) { r5.Nvalue = ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r22 = ret.Where(x => x.RowNum == 22).FirstOrDefault();
                if (r22 != null) { r22.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r23 = ret.Where(x => x.RowNum == 23).FirstOrDefault();
                if (r23 != null) { r23.Nvalue = ret.Where(x => x.RowNum == 1).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        private async Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataCourtProgram9Async(int m1, int m2, int nyear, IEnumerable<int> courtIds)
        {
            var ret = new List<ProgramDataExecutionVm>();

            var programData = await GetProgramDataGridByFilterAsync(9, nyear);
            decimal nVal = 0;
            if (programData.Any())
            {
                foreach (var item in programData)
                {
                    nVal = 0;
                    nVal = await GetKontoMonthDataByCourtIdsValue(9, item?.RowNum ?? 0, m1, m2, nyear, courtIds);



                    var rec = new ProgramDataExecutionVm();
                    rec.Id = item.Id;
                    rec.FunctionalSubAreaId = item.FunctionalSubAreaId;
                    rec.CourtId = 0;
                    rec.RowNum = item.RowNum;
                    rec.PlannedYear = item.PlannedYear;
                    rec.PrnCode = item.PrnCode;
                    rec.Name = item.Name;
                    rec.Nvalue = nVal;
                    rec.ApprovedValue = 0;
                    rec.CalculatedValue = 0;
                    rec.ValueAllowed = item.ValueAllowed ?? false;
                    ret.Add(rec);
                }
                var r4 = ret.Where(x => x.RowNum == 4).FirstOrDefault();
                if (r4 != null) { r4.Nvalue = ret.Where(x => x.RowNum == 5).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 6).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 7).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 8).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r12 = ret.Where(x => x.RowNum == 12).FirstOrDefault();
                if (r12 != null) { r12.Nvalue = ret.Where(x => x.RowNum == 13).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r9 = ret.Where(x => x.RowNum == 9).FirstOrDefault();
                if (r9 != null) { r9.Nvalue = ret.Where(x => x.RowNum == 10).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 11).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }

                var r2 = ret.Where(x => x.RowNum == 2).FirstOrDefault();
                if (r2 != null) { r2.Nvalue = ret.Where(x => x.RowNum == 3).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 9).Select(x => x.Nvalue).FirstOrDefault() ?? 0 + ret.Where(x => x.RowNum == 4).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


                var r1 = ret.Where(x => x.RowNum == 1).FirstOrDefault();
                if (r1 != null) { r1.Nvalue = ret.Where(x => x.RowNum == 2).Select(x => x.Nvalue).FirstOrDefault() ?? 0; }


            }
            return ret;
        }
        public async Task<IEnumerable<ProgramDataExecutionVm>> GetProgramDataCourtGridByIdsAsync(int? functionalSubAreaId, int? m1, int? m2, int? nyear, IEnumerable<int> courtIds)
        {
            switch (functionalSubAreaId)
            {
                case 1: return await GetYearExecutionDataCourtProgram1Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, courtIds);
                case 2: return await GetYearExecutionDataCourtProgram2Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, courtIds);
                case 3: return await GetYearExecutionDataCourtProgram3Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, courtIds);
                case 4: return await GetYearExecutionDataCourtProgram4Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, courtIds);
                case 5: return await GetYearExecutionDataCourtProgram5Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, courtIds);
                case 6: return await GetYearExecutionDataCourtProgram6Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, courtIds);
                case 7: return await GetYearExecutionDataCourtProgram7Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, courtIds);
                case 8: return await GetYearExecutionDataCourtProgram8Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, courtIds);
                case 9: return await GetYearExecutionDataCourtProgram9Async(m1 ?? 0, m2 ?? 0, nyear ?? 0, courtIds);
                default: return new List<ProgramDataExecutionVm>();
            }
        }
       public async Task<int> GetMaxIdFromCourtTypeAsync() {
            var sql = $@"Select Max(Id) from CourtType";
            await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
            await connection.OpenAsync();
            var nSum = connection.ExecuteScalarAsync<int>(sql);

            return (nSum != null) ? nSum.Result : 0;
        }
        public async Task<int> UpdateCourtTypeAsync(CourtTypeVm model) {
            if (model.Id == 0)
            {
                var newId = await GetMaxIdFromCourtTypeAsync();
                var sql = $@"INSERT INTO [dbo].[CourtType]
                          ([Id]
                          ,[Name]
                          ,[InstitutionTypeId])
                    VALUES
                          ({newId + 1}
                          ,'{model?.Name ?? string.Empty}'
                          ,{model?.InstitutionTypeId ?? 0})";
                await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
                await connection.OpenAsync();
                var affectedRows = connection.ExecuteAsync(sql);

                return affectedRows.Result;
            }
            else {
                var sql = $@"Update [dbo].[CourtType] set [Name]='{model?.Name ?? string.Empty}' ,[InstitutionTypeId]={model?.InstitutionTypeId ?? 0} where Id={model.Id}";
                await using SqlConnection connection = (SqlConnection)this._context.CreateConnection();
                await connection.OpenAsync();
                var affectedRows = connection.ExecuteAsync(sql);

                return affectedRows.Result;
            }
        }
     
    }
}
