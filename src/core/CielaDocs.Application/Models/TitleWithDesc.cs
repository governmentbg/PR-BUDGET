using System.Collections.Generic;

namespace CielaDocs.Application.Models
{
    public class TitleWithDesc
    {
        public string Title { get; set; } = string.Empty;
        public List<string> Descr { get; set; } 
    }
}
