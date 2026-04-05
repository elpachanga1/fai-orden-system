using DataRepository.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Services.Domain.Helpers
{
    public class AuthenticationHelper
    {
        private readonly IConfiguration _configuration;

        // Clave secreta para firmar el token
        private readonly string secretKey;
        private readonly int authActivityTime;

        public AuthenticationHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            secretKey = _configuration["auth:secretKey"];
            authActivityTime = int.Parse(_configuration["auth:authActivityTime"]);
        }

        public string Hash(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - devuelve un arreglo de bytes que representa el hash
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convertir el arreglo de bytes a una cadena hexadecimal
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public string? GenerateJWTToken(User? user)
        {
            if (user == null) return null;

            // Creamos las claims (información que queremos incluir en el token)
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            // Creamos las credenciales de seguridad utilizando la clave secreta
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Definimos la información del token (expiración, firma, claims)
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(authActivityTime),
                signingCredentials: creds
            );

            // Generamos el token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            Console.WriteLine("Token JWT generado: ");
            Console.WriteLine(tokenString);

            return tokenString;
        }

        public bool ValidateToken(string tokenString)
        {
            // Parsear y validar el token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = false, // Puedes establecer esto en true y proporcionar el emisor esperado si deseas validar el emisor
                ValidateAudience = false // Puedes establecer esto en true y proporcionar la audiencia esperada si deseas validar la audiencia
            };

            SecurityToken validatedToken;
            try
            {
                // Valida el token y obtiene el resultado
                ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(tokenString, validationParameters, out validatedToken);

                // Si la validación es exitosa, puedes acceder a las claims del token
                foreach (var claim in claimsPrincipal.Claims)
                {
                    Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
                }

                Console.WriteLine("Token JWT válido.");
                return true;
            }
            catch (SecurityTokenValidationException ex)
            {
                Console.WriteLine("Error de validación del token JWT: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al validar el token JWT: " + ex.Message);
                return false;
            }
        }
    }
}
