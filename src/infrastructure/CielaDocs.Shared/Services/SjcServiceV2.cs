using CielaDocs.Application.Models;
using CielaDocs.Shared.Repository;

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

    }
}
