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
    public  class GetFnSubAreaByIdQuery : IRequest<IdNames>
    {
        public int Id { get; set; }

    }
    public class GetFnSubAreaByIdQueryHandler : IRequestHandler<GetFnSubAreaByIdQuery, IdNames>
    {
        private readonly IApplicationDbContext _context;

        public GetFnSubAreaByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<IdNames> Handle(GetFnSubAreaByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await (from a in _context.FunctionalSubArea

                                    where a.Id == request.Id
                                    select new IdNames
                                    {
                                        Id = a.Id,
                                        Name = a.Name
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