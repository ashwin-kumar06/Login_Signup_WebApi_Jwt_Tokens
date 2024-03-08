using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using LoginSignup.Models;
using Microsoft.AspNetCore.Mvc;
using LoginSignup.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;


namespace LoginSignup.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwtsettings;
        public UserController(AppDbContext context, IOptions<JwtSettings> jwtSettings, UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            _context = context;
            _jwtsettings = jwtSettings.Value;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginModel loginuser)

        {
            var user = await _userManager.FindByEmailAsync(loginuser.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginuser.Password))
            {
                return Unauthorized();
            }
            var tokenString = GenerateJwtToken(user);
            return Ok(new { Token = tokenString });
        }

        private string GenerateJwtToken(UserSignUpModel user)

        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtsettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1), // Token expiration time 
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtsettings.Issuer,
                Audience = _jwtsettings.Audience
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] UserSignUpModel signupuser)
        {
            _context.Signup.Add(signupuser);
            _context.SaveChanges();
            return CreatedAtAction(nameof(Signup), new { id = signupuser.Id }, signupuser);
        }
    }
}