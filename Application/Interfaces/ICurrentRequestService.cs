namespace Application.Interfaces;

public interface ICurrentRequestService
{
    string GetClientIp();
    string GetUserAgent();
}