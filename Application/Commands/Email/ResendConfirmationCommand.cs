using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Email;

public class ResendConfirmationCommand : IRequest
{
    public string Email { get; init; }
}