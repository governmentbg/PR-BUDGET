using AutoMapper;

using CielaDocs.Application.Common.Interfaces;

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
    public class GetReportConditionByReportGuidQuery : IRequest<string>
    {
        public string ReportGuid { get; set; }
    }
    public class GetReportConditionByReportGuidQueryHandler : IRequestHandler<GetReportConditionByReportGuidQuery,string>
    {
        private readonly IApplicationDbContext _context;

        public GetReportConditionByReportGuidQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
      
        }
        public async Task<string> Handle(GetReportConditionByReportGuidQuery request, CancellationToken cancellationToken)
        {
            var repo = await _context.Reports.Where(r => r.ReportGuid == request.ReportGuid).FirstOrDefaultAsync();
           
            return repo.ReportCondition;
        }
    }
}
