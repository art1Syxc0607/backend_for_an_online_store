using Application.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.User;

public class DemoteFromAdminHandler : IRequestHandler<DemoteFromAdminCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DemoteFromAdminHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DemoteFromAdminCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (user == null)
            throw new DomainException("User not found");

        if (user.Id == request.AdminId)
            throw new DomainException("Cannot demote yourself");

        // ✅ Проверяем, не последний ли это админ
        var adminCount = await _userRepository.CountAdminsAsync(ct);
        if (user.Role == UserRole.Admin && adminCount <= 1)
            throw new DomainException("Cannot demote the last admin");

        user.DemoteFromAdmin();
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
