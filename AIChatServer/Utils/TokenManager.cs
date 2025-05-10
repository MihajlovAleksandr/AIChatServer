using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AIChatServer.Utils
{
    public static class TokenManager
    {
        private static readonly int expireDays;
        private static readonly int expireMinutes;
        private static readonly string audience;
        private static readonly string issuer;
        private static readonly string secretKey;

        static TokenManager()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var jwtSettings = configuration.GetSection("Jwt");
            secretKey = jwtSettings["Key"];
            issuer = jwtSettings["Issuer"];
            audience = jwtSettings["Audience"];
            expireDays = int.Parse(jwtSettings["ExpireDays"]);
            expireMinutes = int.Parse(jwtSettings["ExpireMinutes"]);

            if (Encoding.UTF8.GetBytes(secretKey).Length < 32)
            {
                throw new ArgumentException("Секретный ключ должен быть не менее 256 бит (32 байта).");
            }
        }

        public static string GenerateToken(int userId, int connectionId)
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


        public static bool ValidateToken(string token, out int userId, out int connectionId, out DateTime expirationTime)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);
            userId = 0;
            connectionId = 0;
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

                    if (int.TryParse(subClaim, out userId) && int.TryParse(connectionIdClaim, out connectionId))
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


        public static string GenerateEntryToken(int userId)
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

        public static bool ValidateEntryToken(string token, out int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);
            userId = 0;

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

                    if (int.TryParse(subClaim, out userId))
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

        public static void DecodeToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            Console.WriteLine("Token claims:");
            foreach (var claim in jwtToken.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }
        }
    }
}