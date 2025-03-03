using CielaDocs.Application.Models;
using CielaDocs.Domain.Entities;
using CielaDocs.Domain.Entities.v2;
using CielaDocs.Shared.Repository;

using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Vml;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Shared.Services
{
    public class SjcServiceV2:ISjcServiceV2
    {
        private readonly ISjcBudgetRepositoryV2 _repo;

        public SjcServiceV2(ISjcBudgetRepositoryV2 budgetRepositoryV2)
        {
            _repo = budgetRepositoryV2;
        }

        public async Task<BudgetPeriodVm> GetActiveBudgetPeriodAsync() { return await _repo.GetActiveBudgetPeriodAsync(); }
        public async Task<IEnumerable<BudgetPeriodVm>> GetInActiveBudgetPeriodsAsync() { return await _repo.GetInActiveBudgetPeriodsAsync(); }
        public async Task<IEnumerable<BudgetPeriodVm>> GetBudgetPeriodsAsync() { return await _repo.GetBudgetPeriodsAsync(); }
        public async Task<BudgetPeriodVm> GetActiveBudgetPeriodByIdAsync(int id) { return await _repo.GetActiveBudgetPeriodByIdAsync(id); }
        public async Task<IEnumerable<ProgramDataHVm>> GetProgramDataForEndingPeriod(int id) { return await _repo.GetProgramDataForEndingPeriod(id); }
        public async Task<bool> GetProgramDataHExistsAsync(int? budgetPeriodId, int? functionalSubAreaId, int? rowNum, int? plannedYear1) { return await _repo.GetProgramDataHExistsAsync(budgetPeriodId, functionalSubAreaId,rowNum,plannedYear1); }
        public async Task<int> InsertIntoProgramDataHAsync(ProgramDataHVm data, int periodId) { return await _repo.InsertIntoProgramDataHAsync(data, periodId); }
        public async Task<bool> GetProgramDataCourtHExistsAsync(int? budgetPeriodId, int? courtId, int? functionalSubAreaId, int? rowNum, int? plannedYear1) { return await _repo.GetProgramDataCourtHExistsAsync(budgetPeriodId,courtId,functionalSubAreaId,rowNum, plannedYear1); }
        public async Task<IEnumerable<ProgramDataCourtHVm>> GetProgramDataCourtForEndingPeriod(int id) { return await _repo.GetProgramDataCourtForEndingPeriod(id); }
        public async Task<int> InsertIntoProgramDataCourtHAsync(ProgramDataCourtHVm data, int periodId) { return await _repo.InsertIntoProgramDataCourtHAsync(data,  periodId); }
        public async Task<bool> GetProgramDataInstitutionHExistsAsync(int? budgetPeriodId, int? institutionTypeId, int? functionalSubAreaId, int? rowNum, int? plannedYear1) { return await _repo.GetProgramDataInstitutionHExistsAsync(budgetPeriodId, institutionTypeId, functionalSubAreaId, rowNum, plannedYear1); }
        public async Task<IEnumerable<ProgramDataInstitutionHVm>> GetProgramDataInstitutionForEndingPeriod(int id) { return await _repo.GetProgramDataInstitutionForEndingPeriod(id); }
        public async Task<int> InsertIntoProgramDataInstitutionHAsync(ProgramDataInstitutionHVm data, int periodId) { return await _repo.InsertIntoProgramDataInstitutionHAsync(data, periodId); }
        public async Task<int?> SpDeleteEndPeriodDataAsync(int budgetPeriodId) { return await _repo.SpDeleteEndPeriodDataAsync(budgetPeriodId); }
        public async Task<IEnumerable<MetricsFieldInProgramVm>> GetMetricsFieldInProgramByMainIndicatorIdAsync(int? id) { return await _repo.GetMetricsFieldInProgramByMainIndicatorIdAsync(id); }
        public async Task<IEnumerable<MetricsFieldInProgramItemVm>> CreateMetricsFieldInProgramItemExists(MainData md) { return await _repo.CreateMetricsFieldInProgramItemExists(md); }
        public async Task<IEnumerable<MetricsFieldInProgramItemVm>> GetMetricsFieldInProgramItemByMainIndicatorsId(int id) { return await _repo.GetMetricsFieldInProgramItemByMainIndicatorsId(id); }
        public async Task<MainIndicatorsVm> GetMainIndicatorsById(int Id) { return await _repo.GetMainIndicatorsById(Id); }

        public async Task<IEnumerable<IndicatorDataHVm>> GetIndicatorDataForEndingPeriod(int id) { return await _repo.GetIndicatorDataForEndingPeriod(id); }
        public async Task<bool> GetIndicatorDataHExistsAsync(int? budgetPeriodId, int? functionalSubAreaId, int? mainIndicatorId, int? plannedYear1) { return await _repo.GetIndicatorDataHExistsAsync(budgetPeriodId, functionalSubAreaId, mainIndicatorId, plannedYear1); }
        public async Task<int> InsertIntoIndicatorDataHAsync(IndicatorDataHVm data, int periodId) { return await _repo.InsertIntoIndicatorDataHAsync(data, periodId); }
        public async Task<bool> GetIndicatorDataCourtHExistsAsync(int? budgetPeriodId, int? courtId, int? functionalSubAreaId, int? mainIndicatorId, int? plannedYear1) { return await _repo.GetIndicatorDataCourtHExistsAsync(budgetPeriodId,courtId, functionalSubAreaId, mainIndicatorId, plannedYear1); }
        public async Task<IEnumerable<IndicatorDataCourtHVm>> GetIndicatorDataCourtForEndingPeriod(int id) { return await _repo.GetIndicatorDataCourtForEndingPeriod(id); }
        public async Task<int> InsertIntoIndicatorDataCourtHAsync(IndicatorDataCourtHVm data, int periodId) { return await _repo.InsertIntoIndicatorDataCourtHAsync(data, periodId); }
        public async Task<int> UpdateIndicatorData3YValueByIdAsync(int? id, string fieldName, decimal? val, int? nYear) { return await _repo.UpdateIndicatorData3YValueByIdAsync(id, fieldName, val, nYear); }
        public async Task<int> UpdateIndicatorDataCourt3YValueByIdAsync(int? id, string fieldName, decimal? val) { return await _repo.UpdateIndicatorDataCourt3YValueByIdAsync(id, fieldName, val); }
       public async Task<IEnumerable<IndicatorDataCourt3Y>> GetIndicatorDataCourt3YAsync(int functionalSubAreaId, int ny, int? mainIndicatorId) { return await _repo.GetIndicatorDataCourt3YAsync(functionalSubAreaId, ny, mainIndicatorId); }
        public async Task<IEnumerable<IndicatorDataCourt3Y>> GetIndicatorDataCourt3YByCourtIdAsync(int? functionalSubAreaId, int? ny, int? courtId) { return await _repo.GetIndicatorDataCourt3YByCourtIdAsync(functionalSubAreaId, ny,courtId); }
        public async Task<IndicatorDataVm> GetIndicatorDataById(int Id) { return await _repo.GetIndicatorDataById(Id); }
        public async Task<IndicatorDataCourtVm> GetIndicatorDataCourtById(int Id) { return await _repo.GetIndicatorDataCourtById(Id); }
    }
}
