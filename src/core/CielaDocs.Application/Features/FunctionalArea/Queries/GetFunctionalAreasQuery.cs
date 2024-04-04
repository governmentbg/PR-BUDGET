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
    public class GetFunctionalAreasQuery : IRequest<IEnumerable<IdNames>>
    {
        public string Name { get; set; }

    }
    public class GetFunctionalAreasQueryHandler : IRequestHandler<GetFunctionalAreasQuery, IEnumerable<IdNames>>
    {
        private readonly IApplicationDbContext _context;

        public GetFunctionalAreasQueryHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<IEnumerable<IdNames>> Handle(GetFunctionalAreasQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await (from a in _context.FunctionalArea

                                    where a.Name.ToUpper().StartsWith(request.Name.ToUpper().Trim())
                                    select new IdNames
                                    {
                                        Id = a.Id,
                                        Name = a.Name
                                    }).Take(20).ToListAsync(cancellationToken);


                return result;
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace.ToString();
                return new List<IdNames>();
            }

        }


    }
}

