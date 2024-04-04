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
    public class GetCountryByIdQuery : IRequest<IdNames> { public int Id { get; set; } }

    public class GetCountryByIdQueryHandler : IRequestHandler<GetCountryByIdQuery, IdNames>
    {
        private readonly IApplicationDbContext _context;

        public GetCountryByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<IdNames> Handle(GetCountryByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _context.Countries.
                    Where(x=>x.Id==request.Id)
                    .Select(x => new IdNames
                {
                    Id = x.Id,
                    Name = x.Name,
                })
                .FirstOrDefaultAsync(cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                //var msg = ex.StackTrace.ToString();
                return new IdNames();
            }

        }


    }
}