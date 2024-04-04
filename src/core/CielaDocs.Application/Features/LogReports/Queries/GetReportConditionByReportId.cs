using AutoMapper;

using CielaDocs.Application.Common.Interfaces;
using CielaDocs.Domain.Entities;

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
    public class GetReportConditionByReportId : IRequest<Reports>
    {
        public int ReportId { get; set; }
    }
    public class GetReportConditionByReportIdHandler : IRequestHandler<GetReportConditionByReportId, Reports>
    {
        private readonly IApplicationDbContext _context;

        public GetReportConditionByReportIdHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;

        }
        public async Task<Reports> Handle(GetReportConditionByReportId request, CancellationToken cancellationToken)
        {
            var repo = await _context.Reports.Where(r => r.Id == request.ReportId).FirstOrDefaultAsync();

            return repo;
        }
    }
}

