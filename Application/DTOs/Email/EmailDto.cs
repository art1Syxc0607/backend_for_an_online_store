using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Email;

public record EmailDto
{
    public string To { get; init; }
    public string Subject { get; init; }
    public string Body { get; init; }
    public bool IsHtml { get; init; } = true;
}