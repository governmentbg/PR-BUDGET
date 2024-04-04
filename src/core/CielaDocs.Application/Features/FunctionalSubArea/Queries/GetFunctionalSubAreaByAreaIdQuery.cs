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
    public class GetFunctionalSubAreaByAreaIdQuery : IRequest<IEnumerable<IdNames>>
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }
    public class GetFunctionalSubAreaByAreaIdQueryHandler : IRequestHandler<GetFunctionalSubAreaByAreaIdQuery, IEnumerable<IdNames>>
    {
        private readonly IApplicationDbContext _context;

        public GetFunctionalSubAreaByAreaIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<IEnumerable<IdNames>> Handle(GetFunctionalSubAreaByAreaIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await (from a in _context.FunctionalSubArea

                                    where a.FunctionalAreaId==request.Id && a.Name.ToUpper().StartsWith(request.Name.ToUpper().Trim())
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
