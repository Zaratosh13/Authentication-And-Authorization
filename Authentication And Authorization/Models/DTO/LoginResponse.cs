using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication_And_Authorization.Models.DTO
{
    public class LoginResponse : Status
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public DateTime? Expiration { get; set; }

        public string? Name { get; set; }

        public string Username { get; set; }
    }
}