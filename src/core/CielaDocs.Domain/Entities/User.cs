using System;
using System.Collections.Generic;

#nullable disable

namespace CielaDocs.Domain.Entities
{
    public partial class User
    {

        public int Id { get; set; }
        public int CourtId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Identifier { get; set; }
        public string Email { get; set; }
        public bool LoginEnabled { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? InactiveFromDate { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? UserTypeId { get; set; }
        public string AspNetUserId { get; set; }
        public string UserName { get; set; }
        public bool CanAdd { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public virtual UserType UserType { get; set; }
        public virtual Court Court { get; set; }

    }
}
