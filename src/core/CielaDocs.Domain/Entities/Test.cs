using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CielaDocs.Domain.Entities
{
    public class Test
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Nvalue { get; set; }
        public bool Disabled { get; set; }
        public bool Calc { get; set; }
    }
}
