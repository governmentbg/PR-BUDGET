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
    public class GetCourtTypesQuery : IRequest<IEnumerable<IdNames>>
    {
        public string Name { get; set; }

    }
    public class GetCourtTypesQueryHandler : IRequestHandler<GetCourtTypesQuery, IEnumerable<IdNames>>
    {
        private readonly IApplicationDbContext _context;

        public GetCourtTypesQueryHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<IEnumerable<IdNames>> Handle(GetCourtTypesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await (from a in _context.CourtTypes
                                   
                                    where  a.Name.ToUpper().StartsWith(request.Name.ToUpper().Trim())
                                    select new IdNames {
                                        Id = a.Id,
                                        Name = a.Name
                                    }).ToListAsync(cancellationToken);


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

