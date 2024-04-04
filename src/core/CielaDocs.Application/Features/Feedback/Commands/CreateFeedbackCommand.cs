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
    public class CreateFeedbackCommand : IRequest<int>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Notes { get; set; }

    }

    public class CCreateFeedbackCommandHandler : IRequestHandler<CreateFeedbackCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CCreateFeedbackCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<int> Handle(CreateFeedbackCommand request, CancellationToken cancellationToken)
        {
            var entry = new Feedback
            {
                Name = request.Name,
                Email = request.Email,
                Notes = request.Notes,
                Datum = DateTime.Now   
            };
            _context.Feedback.Add(entry);
            await _context.SaveChangesAsync(cancellationToken);
            return entry.Id;
        }
    }
}


