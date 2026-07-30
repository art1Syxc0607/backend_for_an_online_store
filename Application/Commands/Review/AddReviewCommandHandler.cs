using Application.Commands.Order;
using Application.Interfaces;
using MediatR;
using Domain.Exceptions;
using Domain.Entities;



namespace Application.Commands.Review;

public class AddReviewCommandHandler : IRequestHandler<AddReviewCommand, int>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddReviewCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork, IUserRepository userRepository, IProductRepository productRepository,
        IReviewRepository reviewRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _reviewRepository = reviewRepository;
    }

    public async Task<int> Handle(AddReviewCommand command, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, ct);
        if (user == null) throw new DomainException("No such user");
        // Проверка подтверждения email
        user.EnsureEmailConfirmed();

        var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
        if (product == null) throw new DomainException("No such product");

        if (!await _orderRepository.HasUserPurchasedProductAsync(command.UserId, command.ProductId, ct))
            throw new DomainException("User didn' buy or recieved this product");

        var review = new Domain.Entities.Review(user, product, command.Text, command.Rating, true);

        await _reviewRepository.AddReviewAsync(review, ct);
        await _unitOfWork.SaveChangesAsync();

        return review.Id;

    }
}
