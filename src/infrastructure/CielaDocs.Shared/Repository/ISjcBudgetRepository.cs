using CielaDocs.Application.Models;
using CielaDocs.Domain.Entities;

using Org.BouncyCastle.Crypto.Engines;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Shared.Repository
{
    public interface ISjcBudgetRepository
    {
        Task<IEnumerable<IdNames>> GetInstitutionsAsync();
        Task<IEnumerable<IdNames>> GetCourtTypeByInstitutionTypeIdAsync(int? institutionTypeId);
        Task<CourtTypeVm> GetCourtTypeVmByIdAsync(int? id);
        Task<IdNames> GetInstitutionTypeByIdAsync(int? typeId);
        Task<IEnumerable<CourtsVm>> GetCourtsAsync();
        Task<IEnumerable<CourtsVm>> GetCourtsByCourtTypeIdAsync(int courtTypeId);
        Task<CourtsVm> GetCourtByKontoCodeAsync(string kontoCode);
        Task<CourtsVm> GetCourtByIdAsync(int id);
        Task<IEnumerable<UsersVm>> GetUsersByCourtTypeIdAsync(int courtTypeId);
        Task<IEnumerable<UsersVm>> GetUsersWithoutAspNetUserIdAsync();
        Task<Domain.Entities.User> GetUserByIdAsync(int id);
        Task<Domain.Entities.User> GetUserByIdentifierAsync(string id);
        Task<Domain.Entities.User> GetUserByASpNetUserIdAsync(string aspNetUserId);
        Task<IEnumerable<Section>> GetSectionsAsync();
        Task<IEnumerable<EbkPar>> GetEbkParBySectionIdAsync(int sectionId);
        Task<IEnumerable<EbkSubPar>> GetEbkSubParByEbkParIdAsync(int ebkParId);
        Task<IEnumerable<TreeListMinDataModel>> GetTreeListEbkMinData();
        Task<IEnumerable<FunctionalArea>> GetFunctionalAreasAsync();
        Task<IEnumerable<FunctionalSubArea>> GetFunctionalSubAreasAsync(int functionalAreaId);
        Task<FunctionalSubArea> GetFunctionalSubAreabyIdAsync(int Id);
        Task<IEnumerable<TreeListMinDataModel>> GetTreeListFunctionalMinData();
        Task<IEnumerable<MainIndicatorsVm>> GetMainIndicatorsByProgramId(int programId);
        Task<IEnumerable<ProgramDefVm>> GetProgramDefByProgramIdAsync(int programId);
        Task<IEnumerable<ProgramDefVm>> GetProgramDefProgCodesByProgramIdAsync(int programId);
        Task<ProgramDefVm> GetProgramDefByIdAsync(int id);
        Task<IEnumerable<FinYear>> GetFinYear();
        Task<IEnumerable<FunctionalAction>> GetFunctionalActionByProgramId(int programId);
        Task<IEnumerable<Metrics>> GetMetricsByProgramId(int programId);
        Task<IEnumerable<MetricsFieldVm>> GetMetricsFields();
        Task<IEnumerable<IdNames>> GetMeasureAsync();
        Task<IEnumerable<IdNames>> GetTypeOfIndicatorAsync();
        Task<IdNames> GetMeasureByIdAsync(int id);
        Task<IdNames> GetTypeOfIndicatorByIdAsync(int id);
        Task<bool> CheckMainDataByCourtIdPeriodAsync(int courtId, int nm, int ny);
        Task<bool> CheckMainDataItemsByCourtIdPeriodAsync(int courtId, int nm, int ny);
        Task<bool> CheckPeriodDataByCourtIdPeriodAsync(int courtId, int nm, int ny);
        Task<bool> CheckPeriodDataItemsByCourtIdPeriodAsync(int courtId, int nm, int ny);

        Task<int?> SpLoadMainDataByCourtIdPeriodAsync(int courtId, int nm, int ny);
        Task<int?> SpLoadMainDataItemsByCourtIdPeriodAsync(int courtId, int nm, int ny);
        Task<int?> SpLoadMainPeriodByCourtIdPeriodAsync(int courtId, int nm, int ny);
        Task<int?> SpLoadMainPeriodItemsByCourtIdPeriodAsync(int courtId, int nm, int ny);

        Task<IEnumerable<MainDataGrid>> GetMainDataGridByFilterAsync(int functionalSubAreaId, int courtId, int nm, int ny);
        Task<IEnumerable<MainDataGrid>> GetMainPeriodGridByFilterAsync(int functionalSubAreaId, int courtId, int nm, int ny);
        Task<IEnumerable<MainDataItemsGrid>> GetMainDataItemsGridByFilterAsync(int courtId, int nm, int ny);
        Task<IEnumerable<MainDataItemsGrid>> GetMainPeriodItemsGridByFilterAsync(int courtId, int nm, int ny);
        Task<MainData> GetMainDataByIdAsync(int Id);
        Task<MainData> GetMainDataPeriodByIdAsync(int Id);
        Task<MainIndicators> GetMainIndicatorsByIdAsync(int Id);
        Task<IEnumerable<MainDataItemsVm>> GetMetricsFiledByCode(int courtId, int nm, int ny, string codes);
        Task<IEnumerable<Test>> GetTestTableAsync();
        Task<IEnumerable<Test2>> GetTest2TableAsync();
        Task<int> UpdateMainDataItemByIdAsync(IEnumerable<MainDataItemsResult> data);
        Task<int> UpdateMainDataItemByIdAsync(IEnumerable<MainDataItemsVm> data);
        Task<int> UpdateMainDataValueByIdAsync(int? Id, double? nValue);
        Task<int> UpdateMainPeriodValueByIdAsync(int? Id, double? nValue);
        Task<int> UpdateTestValue(int id, decimal val);
        Task<int> UpdateTest2Value(int id, decimal val);
        Task<int> UpdateUserWithAspNetUserIdAsync(int id, string s);
        Task<int> UpdateMainDataItemValueByIdAsync(int? id, decimal? val);
        Task<int> UpdateMainPeriodItemValueByIdAsync(int? id, decimal? val);
        Task<int> UpdateMainDataEnteredValueByIdAsync(int? id, decimal? val);
        Task<int> UpdateMainPeriodEnteredValueByIdAsync(int? id, decimal? val);
        Task<IEnumerable<MainDataItemsVm>> GetMainDataItemFiledByCodes(int courtId, int nm, int ny, string codes);
        Task<IEnumerable<MainDataItemsVm>> GetMainPeriodItemFiledByCodes(int courtId, int nm, int ny, string codes);
        Task<int?> Sp_InitFinYearStage1Async(int ny);
        Task<int?> Sp_InitFinYearStage2Async(int ny);
        Task<IEnumerable<ProgramDataGridVm>> GetProgramDataGridByFilterAsync(int functionalSubAreaId, int ny);
        Task<IEnumerable<ProgramData3Y>> GetProgramData3YAsync(int functionalSubAreaId, int ny);
        Task<IEnumerable<ProgramDataCourt3Y>> GetProgramDataCourt3YAsync(int functionalSubAreaId, int ny, int? rowNum);
        Task<IEnumerable<ProgramDataCourt3Y>> GetProgramDataCourt3YByCourtIdAsync(int? programDefNum, int? ny, int? courtId);
        Task<int> UpdateProgramDataValueByIdAsync(int? id, string fieldName, decimal? val);
        Task<int> UpdateProgramData3YValueByIdAsync(int? id, string fieldName, decimal? val);

        Task<int> UpdateProgramDataCourt3YValueByIdAsync(int? id, string fieldName, decimal? val);
        Task<int> UpdateProgramDataCourtValueByIdAsync(int? id, string fieldName, decimal? val);
        Task<IEnumerable<IdNames>> GetCurrencies();
        Task<IEnumerable<IdNames>> GetCurrencyMeasures();
        Task<string> GetNameByIdFromTable(string tableName, int? Id);
        Task<ProgramDataGridVm> GetProgramDataByIdAsync(int? id);
        Task<ProgramDataCourtGridVm> GetProgramDataCourtByIdAsync(int? id);
        Task<IEnumerable<ProgramDataCourtGridVm>> GetProgramDataCourtGridByFilterAsync(int? programDefNum, int? ny, int? rowNum);
        Task<IEnumerable<ProgramDataCourtGridVm>> GetProgramDataCourtGridByCourtIdAsync(int? programDefNum, int? ny, int? courtId);
        Task<IEnumerable<ProgramDataCourtGridVm>> GetProgramDataCourtByCourtIdAsync(int? courtId, int? ny);
        Task<IEnumerable<DraftBudgetDataVm>> GetDraftBudgetDataByCourtIdAsync(int? courtId, int? ny);
        Task<int?> Sp_InitProgramDataAsync(int? programNum, int? ny);
        Task<int?> Sp_InitProgramDataCourtAsync(int? programNum, int? ny);
        Task<int?> Sp_UpdateProgramDataAsync(int? programNum, int? ny);
        Task<int?> Sp_UpdateProgramDataCourtAsync(int? programNum, int? ny);
        Task<IEnumerable<IdNames>> GetCourtsInProgramData(int? programNum);
        Task<string> GetKontoCodesFromProgramDef(int? functionalSubAreaId, int? rowNum);
        Task<string> GetKontoCodesFromDraftBudgetDefByParAsync(string sPar);
        Task<int> AddUpdateKontoMonthData(KontoMonthDataVm data);
        Task<int> AddUpdateDraftBudgetYearDataAsync(DraftBudgetDataVm data);
        Task<int> ProgramDataCourtAsync(int? courtId, int? functionalSubAreaId, int? rowNum, int? nYear);
        Task<int> ProgramDataDraftBudgetCourtAsync(int? courtId, int? functionalSubAreaId, int? rowNum, int? nYear, decimal? nValue);
        Task<int> FirstInitProgramDataDraftBudgetCourtAsync(int? courtId, int? functionalSubAreaId, int? nYear);
        Task<IEnumerable<KontoCourtsYearVm>> GetKontoCourtsYearAsync(int? institutionTypeId, int? courtTypeId, int? courtId, int? ny, int? nmonth, int? reportTypeId);
        Task<int> CalculateProgramDataValues(int functionalSubAreaId, int rowNum, int plannedYear, decimal val);
        Task<int?> sp_RecalculateProgramDataAsync(int? functionalSubAreaId, int? ny);
        Task<int?> sp_RecalculateProgramDataCourtAsync(int? functionalSubAreaId, int? ny, int? courtId);
        Task<int?> sp_UpdateProgramsByProgramDefAsync(int? Id);
        Task<IEnumerable<ProgramDataExecutionVm>> GetYearExecutionDataGridAsync(int? functionalSubAreaId, int? m1, int? m2, int? nyear);
        Task<IEnumerable<ProgramDataExecutionVm>> GetProgramDataCourtGridByFilterAsync(int? functionalSubAreaId, int? m1, int? m2, int? nyear, int? rowNum);

        Task<IEnumerable<string>> GetCourtNamesByIds(IEnumerable<int> ids);
        Task<IEnumerable<ProgramDataExecutionVm>> GetProgramDataCourtGridByIdsAsync(int? functionalSubAreaId, int? m1, int? m2, int? nyear, IEnumerable<int> courtIds);
        Task<IEnumerable<ProgramDataCourtGridVm>> GetProgramDataCourtGridByCourtIdsAsync(int? programDefNum, int? ny, IEnumerable<int> Ids);
        Task<IEnumerable<CourtInProgramVm>> GetCourtInProgramByCourtIdAsync(int? courtId);
        Task<IEnumerable<IdNames>> GetProgramByCourtIdAsync(int? courtId);
        Task<int> GetMaxIdFromCourtTypeAsync();
        Task<int> UpdateCourtTypeAsync(CourtTypeVm model);
        Task<IEnumerable<IdNames>> GetProgramsAsync();
        Task<IEnumerable<ProgramDataInstitution3Y>> GetProgramDataInstitution3YByInstitutionTypeIdAsync(int? programDefNum, int? ny, int? institutionTypeId);
        Task<int?> sp_RecalculateProgramDataInstitutionAsync(int? functionalSubAreaId, int? ny, int? institutionTypeId);
        Task<int> UpdateProgramDataInstitution3YValueByIdAsync(int? id, string fieldName, decimal? val);
        Task<int?> Sp_InitProgramDataInstitutionAsync(int? programNum, int? ny);
        Task<int?> Sp_UpdateProgramDataInstitutionAsync(int? programNum, int? ny);
        Task<IEnumerable<InstitutionInProgramVm>> GetInstitutionInProgramByInstitutionTypeIdAsync(int? institutionTypeId);
        Task<int> FirstInitProgramDataDraftBudgetInstitutionAsync(int? institutionTypeId, int? functionalSubAreaId, int? nYear);
        Task<int> ProgramDataDraftBudgetInstitutionAsync(int? institutionTypeId, int? functionalSubAreaId, int? rowNum, int? nYear, decimal? nValue);
        Task<IEnumerable<IdNames>> GetProgramByInstitutionTypeIdAsync(int? institutionTypeId);
        Task<IEnumerable<ProgramData3Y>> GetProgramData3YTotalAsync(int? programDefNum, int? ny, int? institutionTypeId);
        Task<IEnumerable<IdNames>> GetAllProgramsAsync();
        Task<IEnumerable<ProgramDef3Y>> GetProgramDataCourt3YCommonAsync(int? programDefNum, int? ny);
        Task<IEnumerable<ProgramDef3Y>> GetProgramDataInstitution3YCommonAsync(int? programDefNum, int? ny);
        Task<int?> Sp_InitProgramDataCourtByIdAsync(int? programNum, int? courtId, int? ny);
        Task<IEnumerable<IdNames>> GetCourtInProgramData(int? programNum, int? courtId);
        Task<CfgVm> GetCfgAsync();
    }
  
}
