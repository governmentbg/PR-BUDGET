using System;
using System.Collections.Generic;

#nullable disable

namespace CielaDocs.Domain.Entities
{
    public partial class EkSofrai
    {
        public string Id { get; set; }
        public string TVM { get; set; }
        public string Name { get; set; }
        public string Raion { get; set; }
        public string Kind { get; set; }
        public string Document { get; set; }
    }
}
