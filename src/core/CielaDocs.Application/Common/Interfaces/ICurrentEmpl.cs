using CielaDocs.Application.Dtos;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Common.Interfaces
{
    public interface ICurrentEmpl
    {
        Task<UserDto> GetCurrentEmplAsync();
       
    }
}
