using System;
using System.Collections.Generic;

namespace CielaDocs.Domain.Entities
{
    public partial class DbUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string ObjectId { get; set; }
        public string TenantId { get; set; }
        public bool? IsAdmin { get; set; }
        public string UserPrincipalName { get; set; }
    }
}
