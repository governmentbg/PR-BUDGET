using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Domain.Entities
{
    public partial class Court
    {
        public Court()
        {
            MainData = new HashSet<MainData>();
            MainDataItems = new HashSet<MainDataItem>();
        }

        public int Id { get; set; }
        public int? Num { get; set; }
        public int? CourtTypeId { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public string? CourtGuid { get; set; }
        public string? KontoCode { get; set; }

        public virtual CourtType? CourtType { get; set; }
        public virtual ICollection<MainData> MainData { get; set; }
        public virtual ICollection<MainDataItem> MainDataItems { get; set; }
    }
}
