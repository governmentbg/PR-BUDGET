using CielaDocs.Application.Common.Mappings;
using CielaDocs.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Dtos
{
    public  class UlogDto:IMapFrom<Ulog>
    {
		public long Id { get; set; }
		public int? SchoolId { get; set; }
		public int? EmplId { get; set; }
		public long? CardId { get; set; }
		public int? MsgId { get; set; }
		public string Msg { get; set; }
		public string IP { get; set; }
		public DateTime? CreatedOn { get; set; }
	}
}
