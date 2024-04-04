using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Application.Dtos.DbUser
{
    public class AuthenticateResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }


        public AuthenticateResponse(CielaDocs.Domain.Entities.DbUser user, string token)
        {
            Id = user.Id;
            UserName = user.UserName;
            Token = token;
        }
    }
}
