using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.User;


public class PromoteToAdminHandler : IRequestHandler<PromoteToAdminCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PromoteToAdminHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public PromoteToAdminHandler(IUserRepository userRepository, 
        ILogger<PromoteToAdminHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(PromoteToAdminCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (user == null)
            throw new DomainException("User not found");

        var admin = await _userRepository.GetByIdAsync(request.AdminId, ct);
        if (admin == null)
            throw new DomainException("Admin not found");
        if(admin.Role != Domain.Enums.UserRole.Admin) 
            throw new DomainException("User is not admin");

        // ✅ Нельзя повысить самого себя
        if (user.Id == admin.Id)
            throw new DomainException("Already an admin.");

        var oldRole = user.Role;
        user.PromoteToAdmin();
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogWarning(
        "User promoted to Admin: AdminId {AdminId}, AdminEmail {AdminEmail}, TargetUserId " +
        "{TargetUserId}," +
        " TargetEmail {TargetEmail}, OldRole {OldRole}, NewRole {NewRole}, Time {Time}",
            admin.Id,
            admin.Email,
            user.Id,
            user.Email,
            oldRole,
            user.Role,
            DateTime.UtcNow
        );
    }
}