using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.User;


public class PromoteToAdminHandler : IRequestHandler<PromoteToAdminCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PromoteToAdminHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(PromoteToAdminCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (user == null)
            throw new DomainException("User not found");

        // ✅ Нельзя повысить самого себя
        if (user.Id == request.AdminId)
            throw new DomainException("Cannot promote yourself");

        user.PromoteToAdmin();
        await _unitOfWork.SaveChangesAsync(ct);
    }
}