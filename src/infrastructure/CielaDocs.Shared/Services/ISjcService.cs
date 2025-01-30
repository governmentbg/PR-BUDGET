using CielaDocs.Application.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Shared.Services
{
    public interface ISjcService
    {
        Task<CfgVm> GetCfg();
    }
}
