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
    public class GetTownQuery : IRequest<IEnumerable<IdNames>>
    {
        public string Name { get; set; }

    }
    public class GetTownQueryHandler : IRequestHandler<GetTownQuery, IEnumerable<IdNames>>
    {
        private readonly IApplicationDbContext _context;

        public GetTownQueryHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<IEnumerable<IdNames>> Handle(GetTownQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await (from a in _context.Towns
                                    join n in _context.Municipalities
                                    on a.MunId equals n.MunId
                                    where  a.Name.ToUpper().StartsWith(request.Name.ToUpper().Trim())
                                    select new IdNames {
                                        Id = a.TownId??0,
                                        Name = a.Prefix +" "+ a.Name + " (общ. " + n.Name.Trim() + ")"
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

