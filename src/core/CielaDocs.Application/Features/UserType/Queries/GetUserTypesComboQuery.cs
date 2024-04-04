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
    public class GetUserTypesComboQuery : IRequest<IEnumerable<IdNames>>
    {
        public string Name { get; set; }
    }
    public class GetUserTypesComboQueryHandler : IRequestHandler<GetUserTypesComboQuery, IEnumerable<IdNames>>
    {
        private readonly IApplicationDbContext _context;

        public GetUserTypesComboQueryHandler(IApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<IEnumerable<IdNames>> Handle(GetUserTypesComboQuery request, CancellationToken cancellationToken)
        {

            try
            {


                var result = await _context.UserTypes
                    .Where(x =>x.Name.ToUpper().StartsWith(request.Name.ToUpper().Trim()))
                    .Select(x => new IdNames
                    {
                        Id = x.Id,
                        Name = x.Name,
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

