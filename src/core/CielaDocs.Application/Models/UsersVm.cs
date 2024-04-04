using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class UsersVm
    {
        public int Id { get; set; }
        public int CourtId { get; set; }
        public string UserFullName { get; set; }
        public string Identifier { get; set; }
        public string Email { get; set; }
        public bool LoginEnabled { get; set; }
        public int? UserTypeId { get; set; }
        public string AspNetUserId { get; set; }
        public string UserName { get; set; }
        public bool CanAdd { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public virtual string UserTypeName {get;set;}
        public virtual string CourtName { get;set;}
    }
}
