using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class TokenGenerator : ITokenGenerator
{
    public string GenerateEmailConfirmationToken()
    {
        // Генерируем безопасный токен (256 бит)
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}