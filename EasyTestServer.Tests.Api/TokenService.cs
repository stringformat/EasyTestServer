using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EasyTestServer.Tests.Api.Domain;
using Microsoft.IdentityModel.Tokens;

namespace EasyTestServer.Tests.Api;

internal class TokenService
{
    internal string GenerateToken(User user, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var key = "e1d4152c-5255-459a-9905-7818a04cd6ac"u8.ToArray();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, role),
            }),
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}