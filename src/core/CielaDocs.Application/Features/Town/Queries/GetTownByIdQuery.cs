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
    public class GetTownByIdQuery : IRequest<IdNames>
    {
        public int TownId { get; set; }

    }
    public class GetTownByIdQueryHandler : IRequestHandler<GetTownByIdQuery, IdNames>
    {
        private readonly IApplicationDbContext _context;

        public GetTownByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<IdNames> Handle(GetTownByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await (from a in _context.Towns
                                    join n in _context.Municipalities
                                    on a.MunId equals n.MunId
                                    where a.TownId == request.TownId
                                    select new IdNames
                                    {
                                        Id = a.TownId??0,
                                        Name = a.Prefix + " " + a.Name + " (общ. " + n.Name.Trim() + ")"
                                    }).FirstOrDefaultAsync(cancellationToken);


                return result;
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace.ToString();
                return new IdNames();
            }

        }


    }
}