using CielaDocs.Application.Common.Interfaces;
using CielaDocs.Application.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CielaDocs.Application
{
    public class GetCountriesQuery : IRequest<IEnumerable<IdNames>> { }
    
    public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, IEnumerable<IdNames>>
    {
        private readonly IApplicationDbContext _context;

        public GetCountriesQueryHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<IEnumerable<IdNames>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _context.Countries.Select(x=> new IdNames { 
                    Id = x.Id,
                    Name = x.Name,
                })
                .OrderBy(x=>x.Name)
               .ToListAsync(cancellationToken);

               return result;
            }
            catch (Exception ex)
            {
                //var msg = ex.StackTrace.ToString();
                return new List<IdNames>();
            }

        }


    }
}