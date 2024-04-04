using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class UinVm
    {
        public long Id { get; set; }
        [MaxLength(250)]
        [DisplayName("УИН")]
        public string Uin { get; set; }
    }
}
