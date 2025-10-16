using AIChatServer.Utils.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AIChatServer.Utils.Implementations
{
    public class EntryTokenManager(string secretKey, string issuer, string audience, int expireMinutes) : IEntryTokenManager
    {
        public string GenerateEntryToken(Guid userId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public bool ValidateEntryToken(string token, out Guid userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);
            userId = default;

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var subClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (subClaim == null)
                {
                    subClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                }

                if (subClaim != null)
                {
                    Console.WriteLine($"Sub claim value: {subClaim}");

                    if (Guid.TryParse(subClaim, out userId))
                    {
                        Console.WriteLine($"User ID successfully extracted: {userId}");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Failed to parse user ID from token.");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Sub claim is missing in token.");
                    return false;
                }
            }
            catch (SecurityTokenExpiredException)
            {
                Console.WriteLine("Token has expired.");
                return false;
            }
            catch (SecurityTokenInvalidIssuerException)
            {
                Console.WriteLine("Token issuer is invalid.");
                return false;
            }
            catch (SecurityTokenInvalidAudienceException)
            {
                Console.WriteLine("Token audience is invalid.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Token validation failed: " + ex.Message);
                return false;
            }
        }
    }
}
