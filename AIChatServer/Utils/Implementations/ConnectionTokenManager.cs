using AIChatServer.Utils.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AIChatServer.Utils.Implementations
{
    public class ConnectionTokenManager(string secretKey, string issuer, string audience, int expireDays) : IConnectionTokenManager
    {
        public string GenerateToken(Guid userId, Guid connectionId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim("connectionId", connectionId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expireDays),
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public bool ValidateToken(string token, out Guid userId, out Guid connectionId, out DateTime expirationTime)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);
            userId = default;
            connectionId = default;
            expirationTime = DateTime.MinValue;

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

                var connectionIdClaim = principal.FindFirst("connectionId")?.Value;

                if (subClaim != null && connectionIdClaim != null)
                {
                    Console.WriteLine($"Sub claim value: {subClaim}");
                    Console.WriteLine($"ConnectionId claim value: {connectionIdClaim}");

                    if (Guid.TryParse(subClaim, out userId) && Guid.TryParse(connectionIdClaim, out connectionId))
                    {

                        if (validatedToken is JwtSecurityToken jwtToken)
                        {
                            expirationTime = jwtToken.ValidTo;
                        }

                        Console.WriteLine($"User ID successfully extracted: {userId}");
                        Console.WriteLine($"Connection ID successfully extracted: {connectionId}");
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
                    Console.WriteLine("Sub claim or ConnectionId claim is missing in token.");
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