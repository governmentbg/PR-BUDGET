using CielaDocs.Application.Models;
using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Shared.Services
{
    public interface ISjcServiceV2
    {
        Task<int> GetCurrentYearAsync();
        Task<BudgetPeriodVm> GetActiveBudgetPeriodAsync();
        Task<BudgetPeriodVm> GetActiveBudgetPeriodByIdAsync(int id);
        Task<IEnumerable<BudgetPeriodVm>> GetInActiveBudgetPeriodsAsync();
        Task<IEnumerable<BudgetPeriodVm>> GetBudgetPeriodsAsync();
        Task<IEnumerable<ProgramDataHVm>> GetProgramDataForEndingPeriod(int id);
        Task<bool> GetProgramDataHExistsAsync(int? budgetPeriodId, int? functionalSubAreaId, int? rowNum, int? plannedYear1);
        Task<int> InsertIntoProgramDataHAsync(ProgramDataHVm data, int periodId);
        Task<bool> GetProgramDataCourtHExistsAsync(int? budgetPeriodId,int? courtId, int? functionalSubAreaId, int? rowNum, int? plannedYear1);
        Task<IEnumerable<ProgramDataCourtHVm>> GetProgramDataCourtForEndingPeriod(int id);
        Task<int> InsertIntoProgramDataCourtHAsync(ProgramDataCourtHVm data, int periodId);
        Task<bool> GetProgramDataInstitutionHExistsAsync(int? budgetPeriodId, int? institutionTypeId, int? functionalSubAreaId, int? rowNum, int? plannedYear1);
        Task<IEnumerable<ProgramDataInstitutionHVm>> GetProgramDataInstitutionForEndingPeriod(int id);
        Task<int> InsertIntoProgramDataInstitutionHAsync(ProgramDataInstitutionHVm data, int periodId);
        Task<int?> SpDeleteEndPeriodDataAsync(int budgetPeriodId);
        Task<IEnumerable<MetricsFieldInProgramVm>> GetMetricsFieldInProgramByMainIndicatorIdAsync(int? id);
        Task<IEnumerable<MetricsFieldInProgramItemVm>> CreateMetricsFieldInProgramItemExists(MainData md);
        Task<IEnumerable<MetricsFieldInProgramItemVm>> GetMetricsFieldInProgramItemByMainIndicatorsId(int id, int? courtId, int? nm, int? ny);
        Task<MainIndicatorsVm> GetMainIndicatorsById(int Id);
        Task<IEnumerable<IndicatorDataHVm>> GetIndicatorDataForEndingPeriod(int id);
        Task<bool> GetIndicatorDataHExistsAsync(int? budgetPeriodId, int? functionalSubAreaId, int? mainIndicatorId, int? plannedYear1);
        Task<int> InsertIntoIndicatorDataHAsync(IndicatorDataHVm data, int periodId);
        Task<bool> GetIndicatorDataCourtHExistsAsync(int? budgetPeriodId, int? courtId, int? functionalSubAreaId, int? mainIndicatorId, int? plannedYear1);
        Task<IEnumerable<IndicatorDataCourtHVm>> GetIndicatorDataCourtForEndingPeriod(int id);
        Task<int> InsertIntoIndicatorDataCourtHAsync(IndicatorDataCourtHVm data, int periodId);
        Task<int> UpdateIndicatorData3YValueByIdAsync(int? id, string fieldName, decimal? val, int? nYear);
        Task<int> UpdateIndicatorDataCourt3YValueByIdAsync(int? id, string fieldName, decimal? val);
        Task<IEnumerable<IndicatorDataCourt3Y>> GetIndicatorDataCourt3YAsync(int functionalSubAreaId, int ny, int? mainIndicatorId);
        Task<IEnumerable<IndicatorDataCourt3Y>> GetIndicatorDataCourt3YByCourtIdAsync(int? functionalSubAreaId, int? ny, int? courtId);
        Task<IndicatorDataVm> GetIndicatorDataById(int Id);
        Task<IndicatorDataCourtVm> GetIndicatorDataCourtById(int Id);
        Task<IEnumerable<IndicatorDataCourt1Y>> GetIndicatorDataCourt1YAsync(int functionalSubAreaId, int ny, int? mainIndicatorId);
        Task<int> UpdateIndicatorData1YValueByIdAsync(int? id, string fieldName, decimal? val, int? nYear);
        Task<int> UpdateIndicatorDataCourt1YValueByIdAsync(int? id, string fieldName, decimal? val);
        Task<IEnumerable<IndicatorDataCourt1Y>> GetIndicatorDataCourt1YByCourtIdAsync(int? functionalSubAreaId, int? ny, int? courtId);



    }
}
