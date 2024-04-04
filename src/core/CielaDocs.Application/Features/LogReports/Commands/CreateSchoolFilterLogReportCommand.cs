using CielaDocs.Application.Common.Interfaces;
using CielaDocs.Domain.Entities;

using MediatR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CielaDocs.Application
{
    public class CreateOnrFilterLogReportCommand : IRequest<int>
    {
        public string ReportGuid { get; set; }
        public string ReportCondition { get; set; }
        public string ReportTitle { get; set; }
        public string ReportSubTitle { get; set; }
    }

    public class CreateOnrFilterLogReportCommandHandler : IRequestHandler<CreateOnrFilterLogReportCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateOnrFilterLogReportCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<int> Handle(CreateOnrFilterLogReportCommand request, CancellationToken cancellationToken)
        {
            var entry = new CielaDocs.Domain.Entities.Reports
            {
                ReportGuid = request?.ReportGuid ?? string.Empty,
                ReportCondition = request?.ReportCondition ?? string.Empty,
                ReportTitle = request?.ReportTitle ?? string.Empty,
                ReportSubTitle = request?.ReportSubTitle ?? string.Empty
            };
            try
            {
                _context.Reports.Add(entry);
                await _context.SaveChangesAsync(cancellationToken);
                return entry.Id;
            }
            catch (Exception ex)
            {
                
                return 0;
            }
        }
    }
}


