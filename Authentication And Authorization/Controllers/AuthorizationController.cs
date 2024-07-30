// Ignore Spelling: Admin

using Authentication_And_Authorization.Data;
using Authentication_And_Authorization.Models.Domain;
using Authentication_And_Authorization.Models.DTO;
using Authentication_And_Authorization.Repositories.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Authentication_And_Authorization.Controllers
{
    [ApiController]
    [Route("api/[controller]/{action}")]
    public class AuthorizationController : ControllerBase
    {
        private readonly AuThDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;

        public AuthorizationController(AuThDbContext context, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager, ITokenService tokenService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword changePassword)
        {
            var status = new Status();
            if (ModelState.IsValid)
            {
                status.StatusCode = 0;
                status.Message = "Please all the fields!";
                return Ok(status);
            }

            //Lets find the user
            var user = await _userManager.FindByNameAsync(changePassword.Username);
            if (user == null)
            {
                status.StatusCode = 0;
                status.Message = "Please pass all the valid fields";
                return Ok(status);
            }

            //Check current password
            if (!await _userManager.CheckPasswordAsync(user, changePassword.CurrentPassword))
            {
                status.StatusCode = 0;
                status.Message = "Invalid current Password";
                return Ok(status);
            }

            //Change password
            var reuslt = await _userManager.ChangePasswordAsync(user, changePassword.CurrentPassword, changePassword.NewPassword);
            if (!reuslt.Succeeded)
            {
                status.StatusCode = 0;
                status.Message = "Failed to change password";
                return Ok(status);
            }
            status.StatusCode = 0;
            status.Message = "Password has changed Successfully!";
            return Ok(reuslt);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var user = await _userManager.FindByNameAsync(login.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, login.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = _tokenService.GetToken(authClaims);
                var refreshToken = _tokenService.GetRefreshToken();
                var TokenInfo = _context.TokenInfo.FirstOrDefault(a => a.Username == user.UserName);

                if (TokenInfo == null)
                {
                    var info = new TokenInfo
                    {
                        Username = user.UserName,
                        RefreshToken = refreshToken,
                        RefreshTokenExpiry = DateTime.Now.AddDays(7)
                    };
                    _context.TokenInfo.Add(info);
                }
                else
                {
                    TokenInfo.RefreshToken = refreshToken;
                    TokenInfo.RefreshTokenExpiry = DateTime.Now.AddDays(7);
                }

                _context.SaveChanges();

                return Ok(new LoginResponse
                {
                    Name = user.Name,
                    Username = user.UserName,
                    Token = token.TokenString,
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo,
                    StatusCode = 1,
                    Message = "Logged in!"
                });
            }
            return Ok(
            new LoginResponse
            {
                StatusCode = 0,
                Message = "Invalid Username or Password",
                Token = "",
                Expiration = null
            });
        }

        [HttpPost]
        public async Task<IActionResult> Registration([FromBody] Registration registration)
        {
            var status = new Status();
            if (!ModelState.IsValid)
            {
                status.StatusCode = 0;
                status.Message = "Please fill all required fields";
                return Ok(status);
            }

            //check of user exist
            var userExist = await _userManager.FindByNameAsync(registration.Username);
            if (userExist != null)
            {
                status.StatusCode = 0;
                status.Message = "User already exist";
                return Ok(status);
            }

            var user = new ApplicationUser()
            {
                UserName = registration.Username,
                SecurityStamp = Guid.NewGuid().ToString(),
                Email = registration.Email,
                Name = registration.Name
            };

            //create a new user
            var result = await _userManager.CreateAsync(user, registration.Password);
            if (!result.Succeeded)
            {
                status.StatusCode = 0;
                status.Message = "User Creation Failed";
                return Ok(status);
            }

            //add roles here
            // for admin registration we have to replace userRoles.Admin from userRoles.user
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            if (await _roleManager.RoleExistsAsync(UserRoles.User))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }
            status.StatusCode = 1;
            status.Message = "Successfully registered!";
            return Ok(status);
        }

        [HttpPost]
        //public async Task<IActionResult> RegistrationAdmin([FromBody] Registration registration)
        //{
        //    var status = new Status();
        //    if (!ModelState.IsValid)
        //    {
        //        status.StatusCode = 0;
        //        status.Message = "Please fill all required fields";
        //        return Ok(status);
        //    }

        //    //check of user exist
        //    var userExist = await _userManager.FindByNameAsync(registration.Username);
        //    if (userExist != null)
        //    {
        //        status.StatusCode = 0;
        //        status.Message = "User already exist";
        //        return Ok(status);
        //    }

        //    var user = new ApplicationUser()
        //    {
        //        UserName = registration.Username,
        //        SecurityStamp = Guid.NewGuid().ToString(),
        //        Email = registration.Email,
        //        Name = registration.Name
        //    };

        //    //create a new user
        //    var result = await _userManager.CreateAsync(user, registration.Password);
        //    if (!result.Succeeded)
        //    {
        //        status.StatusCode = 1;
        //        status.Message = "User Successfully created";
        //        return Ok(status);
        //    }

        //    //add roles here
        //    // for admin registration we have to replace userRoles.Admin from userRoles.user
        //    if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
        //        await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
        //    if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
        //    {
        //        await _userManager.AddToRoleAsync(user, UserRoles.Admin);
        //    }
        //    status.StatusCode = 1;
        //    status.Message = "Successfully registered!";
        //    return Ok(status);
        //}
    }
}


