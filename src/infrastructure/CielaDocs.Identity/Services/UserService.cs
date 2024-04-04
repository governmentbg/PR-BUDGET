using CielaDocs.Application.Dtos.DbUser;
using CielaDocs.Domain.Entities;
using CielaDocs.Identity.Helpers;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CielaDocs.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly List<DbUser> _users = new List<DbUser>
        {
            new DbUser
            {
                Id = 1,
              UserName = "a.tinkin@abv.bg",
              UserPassword = "Pass123!"
            }
        };

        private readonly AuthUserSettings _authSettings;
        public UserService(IOptions<AuthUserSettings> appSettings) => _authSettings = appSettings.Value;

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = _users.SingleOrDefault(u => u.UserName == model.UserName && u.UserPassword == model.UserPassword);

            if (user == null)
                return null;

            var token = GenerateJwtToken(user);

            return new AuthenticateResponse(user, token);
        }


        public DbUser GetById(int id) => _users.FirstOrDefault(u => u.Id == id);

        private string GenerateJwtToken(DbUser user)
        {
            byte[] key = Encoding.ASCII.GetBytes(_authSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("sub", user.Id.ToString()), new Claim("email", user.UserName) }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}