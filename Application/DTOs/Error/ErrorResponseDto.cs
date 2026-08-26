using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Error;


public class ErrorResponseDto
{
    public bool Success { get; set; } = false;
    public string Error { get; set; }
    public string ErrorCode { get; set; }
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Path { get; set; }
    public string? TraceId { get; set; }
    public List<ValidationError>? ValidationErrors { get; set; }
}

public class ValidationError
{
    public string Field { get; set; }
    public string Message { get; set; }
}