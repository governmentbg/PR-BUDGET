using System;

namespace CielaDocs.Application.Models
{
    public class EmplLogVm
    {
        public int EmplId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReportGuid { get; set; }
    }
}
