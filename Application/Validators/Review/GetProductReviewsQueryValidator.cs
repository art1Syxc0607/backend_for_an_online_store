using Application.Common;
using Application.Queries.Review;
using FluentValidation;

namespace Application.Validators.Review;

public class GetProductReviewsQueryValidator : AbstractValidator<GetProductReviewsQuery>
{
    public GetProductReviewsQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID is required");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5")
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.MinRating)
            .InclusiveBetween(1, 5).WithMessage("Minimum rating must be between 1 and 5")
            .When(x => x.MinRating.HasValue);

        RuleFor(x => x.ToDate)
            .Must((query, toDate) => DateRange.IsValid(query.FromDate, toDate))
            .WithMessage("From date cannot be later than to date");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("Page size must be between 1 and 50");
    }
}
