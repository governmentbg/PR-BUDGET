using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class ApplicationLogVm
    {
      
            public long Id { get; set; }
            public int? EmplId { get; set; }
             public string Msg { get; set; }
            public string IP { get; set; }
            public DateTime? CreatedOn { get; set; }
            public string? FullName { get; set; }


    }
}
