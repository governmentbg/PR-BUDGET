using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Models
{
    public class UserLockVm
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? UserLockedItemId { get; set; }
    }
}
