using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using LoginSignup.Data;
using LoginSignup.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace LoginSignup.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwtsettings;
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public UserController(AppDbContext context, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtsettings = jwtSettings.Value;
            _key = Encoding.ASCII.GetBytes(_jwtsettings.Key);
            _iv = new byte[16];

        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginModel loginuser)
        {
            
            var user = _context.Signup.FirstOrDefault(u => u.Email == loginuser.Email);
            if (user == null)
            {
                return NotFound("Email or Password Not Found");
            }
            string decryptedPassword = DecryptString(user.Password);
            if ( decryptedPassword != loginuser.Password)
            {
                return NotFound("Invalid Email or Password");
            }
            var tokenString = GenerateJwtToken(user);
            return Ok(new { tokenString });
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
            signupuser.Password = EncryptString(signupuser.Password);
            signupuser.ConfirmPassword = EncryptString(signupuser.ConfirmPassword);
            _context.Signup.Add(signupuser);
            _context.SaveChanges();
            return CreatedAtAction(nameof(Signup), new { id = signupuser.Id }, signupuser);
        }

        private string EncryptString(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        private string DecryptString(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}