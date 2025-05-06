using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class AuthResult
    {
        public bool IsAuthenticated { get; set; }
        public string UserIdentifier { get; set; }
        public string ErrorDescription { get; set; }
    }
}
