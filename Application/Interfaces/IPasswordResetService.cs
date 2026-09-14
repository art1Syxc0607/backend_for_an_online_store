
namespace Application.Interfaces;

public interface IPasswordResetService
{
    string GenerateCode();
    string HashCode(string code);
    bool VerifyCode(string code, string hash);
}