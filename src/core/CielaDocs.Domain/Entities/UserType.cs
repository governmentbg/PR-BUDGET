using System;
using System.Collections.Generic;

#nullable disable

namespace CielaDocs.Domain.Entities
{
    public partial class UserType
    {
        //public UserType()
        //{
        //    Users = new HashSet<User>();
        //}

        public int Id { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
