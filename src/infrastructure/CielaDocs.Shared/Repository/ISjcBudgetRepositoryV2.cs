using CielaDocs.Application.Models;
using CielaDocs.Domain.Entities;
using CielaDocs.Domain.Entities.v2;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Shared.Repository
{
    public interface ISjcBudgetRepositoryV2
    {
        Task<IEnumerable<BudgetPeriodVm>> GetBudgetPeriodsAsync();
        Task<BudgetPeriodVm> GetActiveBudgetPeriodAsync();
        Task<BudgetPeriodVm> GetActiveBudgetPeriodByIdAsync(int id);
        Task<IEnumerable<BudgetPeriodVm>> GetInActiveBudgetPeriodsAsync();
        Task<IEnumerable<ProgramDataHVm>> GetProgramDataForEndingPeriod(int id);
        Task<bool> GetProgramDataHExistsAsync(int? budgetPeriodId, int? functionalSubAreaId, int? rowNum, int? plannedYear1);
        Task<int> InsertIntoProgramDataHAsync(ProgramDataHVm data, int periodId);
        Task<bool> GetProgramDataCourtHExistsAsync(int? budgetPeriodId, int? courtId, int? functionalSubAreaId, int? rowNum, int? plannedYear1);
        Task<IEnumerable<ProgramDataCourtHVm>> GetProgramDataCourtForEndingPeriod(int id);
        Task<int> InsertIntoProgramDataCourtHAsync(ProgramDataCourtHVm data, int periodId);

        Task<bool> GetProgramDataInstitutionHExistsAsync(int? budgetPeriodId, int? institutionTypeId, int? functionalSubAreaId, int? rowNum, int? plannedYear1);
        Task<IEnumerable<ProgramDataInstitutionHVm>> GetProgramDataInstitutionForEndingPeriod(int id);
        Task<int> InsertIntoProgramDataInstitutionHAsync(ProgramDataInstitutionHVm data, int periodId);
        Task<int?> SpDeleteEndPeriodDataAsync(int budgetPeriodId);
    }
}
