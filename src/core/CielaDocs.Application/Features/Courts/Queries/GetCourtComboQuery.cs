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
    public class GetCourtComboQuery : IRequest<IEnumerable<IdNames>> { public string Name { get; set; } }

    public class GetCourtComboQueryHandler : IRequestHandler<GetCourtComboQuery, IEnumerable<IdNames>>
    {
        private readonly IApplicationDbContext _context;

        public GetCourtComboQueryHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<IEnumerable<IdNames>> Handle(GetCourtComboQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _context.Courts
                    .Where(x=>x.Name.ToUpper().StartsWith(request.Name.ToUpper().Trim()))
                    .Select(x => new IdNames
                    {
                        Id = x.Id,
                        Name =x.Name,
                    })
                .OrderBy(x => x.Name)
                .Take(20)
                .ToListAsync(cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                return new List<IdNames>();
            }
        }
    }
}