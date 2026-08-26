
namespace Domain.Exceptions;

//public class DomainException : Exception
//{
//    public DomainException(string message) : base(message) { }
//}

public class DomainException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    public DomainException(string message, string errorCode = "DOMAIN_ERROR", int statusCode = 400)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
