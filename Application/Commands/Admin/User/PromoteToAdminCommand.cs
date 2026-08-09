using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.User;

public class PromoteToAdminCommand : IRequest
{
    public int UserId { get; init; }
    public int AdminId { get; init; } // ID администратора, который выполняет действие
}