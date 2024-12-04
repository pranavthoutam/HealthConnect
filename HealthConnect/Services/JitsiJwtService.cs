using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;

namespace HealthConnect.Services
{
    public static class JitsiJwtService
    {
        public static string GenerateJitsiMeetingLink(string room, DateTime startTime, DateTime expiryTime, UserManager<User> userManager, ClaimsPrincipal currentUser)
        {
            // Ensure appSecret is at least 256 bits (32 bytes)
            string appSecret = "G2O9sItG9xM8t4k9hFl5A1V9zyqaG8JXvfDmsXTr3wE=";  // Your app secret or a properly sized secret
            if (appSecret.Length < 32)
            {
                appSecret = appSecret.PadRight(32, '0');  // Pad the key to ensure it is 256 bits
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Add buffer to the startTime and expiryTime
            var buffer = TimeSpan.FromSeconds(10);  // 10-second buffer

            long nbf = new DateTimeOffset(startTime.Add(buffer)).ToUnixTimeSeconds();
            long exp = new DateTimeOffset(expiryTime.Add(buffer)).ToUnixTimeSeconds();

            // Retrieve the user ID and name from the current user (using Identity)
            var userId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);  // User ID from claims
            var userName = currentUser.Identity.Name;  // User Name from Identity

            var claims = new List<Claim>
            {
                new Claim("aud", "jitsi"),
                new Claim("iss", "G2O9sItG9xM8t4k9hFl5A1V9zyqaG8JXvfDmsXTr3wE="),  // Replace with your Jitsi app ID
                new Claim("sub", "meet.jit.si"),
                new Claim("room", room),
                new Claim("exp", exp.ToString(), ClaimValueTypes.Integer64),
                new Claim("nbf", nbf.ToString(), ClaimValueTypes.Integer64),
                new Claim("context", JsonConvert.SerializeObject(new
                {
                    user = new
                    {
                        id = userId,  // User ID from Identity
                        name = userName  // User Name from Identity
                    }
                }))
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime,
                NotBefore = DateTimeOffset.FromUnixTimeSeconds(nbf).UtcDateTime,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);

            return $"https://meet.jit.si/{room}?jwt={tokenHandler.WriteToken(securityToken)}";
        }
    }
}
