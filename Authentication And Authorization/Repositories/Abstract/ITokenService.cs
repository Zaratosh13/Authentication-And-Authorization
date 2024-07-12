using Authentication_And_Authorization.Models.DTO;
using System.Security.Claims;

namespace Authentication_And_Authorization.Repositories.Abstract
{
    public interface ITokenService
    {
        TokenResponse GetToken(IEnumerable<Claim> claims);

        String GetRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}