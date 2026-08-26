using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "You are not authorized")
        : base(message, "UNAUTHORIZED", 401)
    { }
}