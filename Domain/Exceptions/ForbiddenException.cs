using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions;

public class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "You don't have permission")
        : base(message, "FORBIDDEN", 403)
    { }
}
