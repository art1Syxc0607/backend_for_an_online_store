using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Application.Interfaces;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class PasswordResetService : IPasswordResetService
{
    public string GenerateCode()
    {
        // ✅ Криптографически стойкий 6-значный код
        var bytes = RandomNumberGenerator.GetBytes(4);
        var number = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
        return number.ToString("D6");
    }

    public string HashCode(string code)
    {
        // ✅ Используем BCrypt или Argon2 (не SHA256!)
        return BCrypt.Net.BCrypt.HashPassword(code);
    }

    public bool VerifyCode(string code, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(code, hash);
    }
}