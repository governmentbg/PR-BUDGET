using CielaDocs.Application.Models;
using CielaDocs.Shared.Repository;

using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

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
    }
}
