using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions;

public class ValidationException : DomainException
{
    public List<string> Errors { get; }

    public ValidationException(List<string> errors)
        : base("Validation failed", "VALIDATION_ERROR", 400)
    {
        Errors = errors;
    }
}