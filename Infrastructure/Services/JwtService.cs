using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly JwtOptions _options;

    public JwtService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateToken(User user)
    {

        var claims = new List<Claim> // for tests
        {
            // Используем JwtRegisteredClaimNames вместо ClaimTypes
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.UserName),
            new Claim("userId", user.Id.ToString()),

             new Claim(ClaimTypes.Role, user.Role.ToString()), // "User" или "Admin"
            // или для совместимости с некоторыми системами:
            // new Claim("role", user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            //expires: DateTime.UtcNow.AddHours(1),
            //expires: DateTime.UtcNow.AddDays(7),
            expires: DateTime.UtcNow.AddMonths(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}