using CielaDocs.Application.Models;
using CielaDocs.Shared.Repository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Shared.Services
{
    public class SjcService:ISjcService
    {
        private readonly ISjcBudgetRepository _repo;

        public SjcService(ISjcBudgetRepository budgetRepository)
        {
            _repo=budgetRepository;
        }

        public async Task<CfgVm> GetCfg() { return await _repo.GetCfgAsync(); }
        
    }
}
