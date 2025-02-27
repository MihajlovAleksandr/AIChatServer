using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;

namespace AIChatServer
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

            // Получение настроек JWT
            var jwtSettings = configuration.GetSection("Jwt");
            secretKey = jwtSettings["Key"];
            issuer = jwtSettings["Issuer"];
            audience = jwtSettings["Audience"];
            expireDays = int.Parse(jwtSettings["ExpireDays"]);
            expireMinutes = int.Parse(jwtSettings["ExpireMinutes"]);

            // Проверка длины ключа
            if (Encoding.UTF8.GetBytes(secretKey).Length < 32)
            {
                throw new ArgumentException("Секретный ключ должен быть не менее 256 бит (32 байта).");
            }
        }

        public static string GenerateToken(int userId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // Создание подписывающих учетных данных
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Создание утверждений (claims)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), // Добавляем userId в claim "sub"
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            };

            // Создание JWT токена
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expireDays),
                signingCredentials: creds
            );

            // Генерация строки токена
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
        public static string GenerateEntryToken(int userId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // Создание подписывающих учетных данных
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Создание утверждений (claims)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), // Добавляем userId в claim "sub"
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            };

            // Создание JWT токена
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireDays),
                signingCredentials: creds
            );

            // Генерация строки токена
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public static bool ValidateToken(string token, out int userId, out DateTime expirationTime)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);
            userId = 0; // Инициализация userId значением по умолчанию
            expirationTime = DateTime.MinValue; // Инициализация expirationTime значением по умолчанию

            try
            {
                // Валидация токена
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // Отключаем ClockSkew
                }, out SecurityToken validatedToken);

                // Извлечение userId из claims
                var subClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Используем ClaimTypes.NameIdentifier
                if (subClaim == null)
                {
                    subClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value; // Альтернативно используем JwtRegisteredClaimNames.Sub
                }

                if (subClaim != null)
                {
                    Console.WriteLine($"Sub claim value: {subClaim}"); // Логирование значения sub claim
                    if (int.TryParse(subClaim, out userId))
                    {
                        Console.WriteLine($"User ID successfully extracted: {userId}"); // Логирование успешного извлечения userId

                        // Извлечение времени истечения срока действия токена
                        if (validatedToken is JwtSecurityToken jwtToken)
                        {
                            expirationTime = jwtToken.ValidTo;
                        }

                        return true; // Токен валиден, userId успешно извлечен
                    }
                    else
                    {
                        Console.WriteLine("Failed to parse user ID from token.");
                        return false; // Токен валиден, но userId не удалось извлечь
                    }
                }
                else
                {
                    Console.WriteLine("Sub claim is missing in token.");
                    return false; // Токен валиден, но claim sub отсутствует
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