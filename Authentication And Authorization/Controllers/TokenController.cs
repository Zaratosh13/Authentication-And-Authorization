using Authentication_And_Authorization.Data;
using Authentication_And_Authorization.Models.DTO;
using Authentication_And_Authorization.Repositories.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication_And_Authorization.Controllers
{
    [Route("api/[controller]/{action}")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly AuThDbContext _auThDbContext;
        private readonly ILogger<TokenController> _logger;

        public TokenController(ITokenService tokenService, AuThDbContext auThDbContext, ILogger<TokenController> logger)
        {
            this._tokenService = tokenService;
            this._auThDbContext = auThDbContext;
            this._logger = logger;
        }

        [HttpPost]
        public IActionResult Refresh(RefreshTokenRequest refreshTokens)
        {
            try
            {
                if (refreshTokens is null)
                    return BadRequest("Invalid client request");

                string accessToken = refreshTokens.AccessToken;
                string refreshToken = refreshTokens.RefreshToken;

                var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
                var username = principal.Identity.Name;
                var user = _auThDbContext.TokenInfo.SingleOrDefault(u => u.Username == username);

                if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.Now)
                    return BadRequest("Invalid client request");

                var newAccessToken = _tokenService.GetToken(principal.Claims);
                var newRefreshToken = _tokenService.GetRefreshToken();
                user.RefreshToken = newRefreshToken;
                _auThDbContext.SaveChanges();

                return Ok(new RefreshTokenRequest()
                {
                    AccessToken = newAccessToken.TokenString,
                    RefreshToken = newRefreshToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while refreshing tokens");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost, Authorize]
        public IActionResult Revoke()
        {
            try
            {
                var username = User.Identity.Name;
                var user = _auThDbContext.TokenInfo.SingleOrDefault(u => u.Username == username);

                if (user is null)
                    return BadRequest("Invalid client request");

                user.RefreshToken = null;
                _auThDbContext.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while revoking token");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
