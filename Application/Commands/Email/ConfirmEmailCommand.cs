using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Email;

public class ConfirmEmailCommand : IRequest
{
    public int UserId { get; init; }
    public string Token { get; init; }
}
