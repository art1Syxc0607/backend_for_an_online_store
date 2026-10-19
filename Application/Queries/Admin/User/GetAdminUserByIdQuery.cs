using Application.DTOs.Admin.User;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Admin.User;

// ✅ Админский запрос полной информации о пользователе по ID
public class GetAdminUserByIdQuery : IRequest<AdminUserDetailDto?>
{
    public int UserId { get; init; }
}
