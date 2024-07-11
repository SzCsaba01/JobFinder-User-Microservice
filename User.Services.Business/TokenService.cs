using User.Data.Access.Helpers;
using User.Data.Object.Entities;
using User.Services.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace User.Services.Business;
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> GenerateRandomTokenAsync()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            var randomBytes = new byte[32];
            rng.GetBytes(randomBytes);

            var base64UrlToken = Base64UrlEncoder.Encode(randomBytes);

            return await Task.FromResult(base64UrlToken);
        }
    }

    public async Task<string> GetAuthentificationJwtAsync(UserEntity user)
    {
        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim("Id", user.UserProfile.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.RoleName),
        });

        var expirationDate = DateTime.UtcNow.AddHours(AppConstants.JWT_TOKEN_VALIDATION_TIME);
        
        return GenerateJwtToken(claimsIdentity, expirationDate);
    }

    private  string GenerateJwtToken(ClaimsIdentity claims, DateTime expirationDate)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration.GetSection("Jwt:Key").Value);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claims,
            Expires = expirationDate,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
