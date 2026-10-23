using Application.Commands.Cart;
using Application.Commands.Order;
using Application.Commands.Product;
using Application.Commands.Review;
using Application.Commands.User;
using Application.DTOs.File;
using Application.DTOs.Order;
using Application.Queries.Admin.Dashboard;
using Application.Queries.Admin.Order;
using Application.Queries.Order;
using Application.Queries.Review;
using Application.Validators.Admin.Dashboard;
using Application.Validators.Admin.Order;
using Application.Validators.Cart;
using Application.Validators.Order;
using Application.Validators.Product;
using Application.Validators.Review;
using Application.Validators.User;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace ApplicationTests.Validation;

public class ValidatorTests
{
    // ── ResetPassword ──────────────────────────────────────────────

    [Fact]
    public void ResetPassword_WhenPasswordsDoNotMatch_ShouldHaveError()
    {
        var validator = new ResetPasswordCommandValidator();

        var result = validator.TestValidate(new ResetPasswordCommand
        {
            Email = "user@mail.com",
            Code = "123456",
            NewPassword = "Password123",
            ConfirmPassword = "Another123"
        });

        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Passwords do not match");
    }

    [Fact]
    public void ResetPassword_WhenPasswordIsWeak_ShouldHaveError()
    {
        var validator = new ResetPasswordCommandValidator();

        var result = validator.TestValidate(new ResetPasswordCommand
        {
            Email = "user@mail.com",
            Code = "123456",
            NewPassword = "123",
            ConfirmPassword = "123"
        });

        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void ResetPassword_WhenCodeIsNotSixDigits_ShouldHaveError()
    {
        var validator = new ResetPasswordCommandValidator();

        var result = validator.TestValidate(new ResetPasswordCommand
        {
            Email = "user@mail.com",
            Code = "abc",
            NewPassword = "Password123",
            ConfirmPassword = "Password123"
        });

        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void ResetPassword_WhenDataIsValid_ShouldNotHaveErrors()
    {
        var validator = new ResetPasswordCommandValidator();

        var result = validator.TestValidate(new ResetPasswordCommand
        {
            Email = "user@mail.com",
            Code = "123456",
            NewPassword = "Password123",
            ConfirmPassword = "Password123"
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── Cart ───────────────────────────────────────────────────────

    [Fact]
    public void AddToCart_WhenQuantityIsZero_ShouldHaveError()
    {
        var validator = new AddToCartCommandValidator();

        var result = validator.TestValidate(new AddToCartCommand
        {
            UserId = 1,
            ProductId = 1,
            Quantity = 0
        });

        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void UpdateCartQuantity_WhenQuantityIsNegative_ShouldHaveError()
    {
        var validator = new UpdateCartItemQuantityCommandValidator();

        var result = validator.TestValidate(new UpdateCartItemQuantityCommand
        {
            UserId = 1,
            ProductId = 1,
            NewQuantity = -1
        });

        result.ShouldHaveValidationErrorFor(x => x.NewQuantity);
    }

    // ── Orders ─────────────────────────────────────────────────────

    [Fact]
    public void CreateOrder_WhenItemsAreEmpty_ShouldHaveError()
    {
        var validator = new CreateOrderCommandValidator();

        var result = validator.TestValidate(new CreateOrderCommand
        {
            UserId = 1,
            ShippingAddress = "Some valid address",
            BaseUrl = "http://localhost",
            Items = new List<CreateOrderItemDto>()
        });

        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void CreateOrder_WhenItemQuantityIsInvalid_ShouldHaveError()
    {
        var validator = new CreateOrderCommandValidator();

        var result = validator.TestValidate(new CreateOrderCommand
        {
            UserId = 1,
            ShippingAddress = "Some valid address",
            BaseUrl = "http://localhost",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = 1, Quantity = 0 }
            }
        });

        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Fact]
    public void CreateOrder_WhenDataIsValid_ShouldNotHaveErrors()
    {
        var validator = new CreateOrderCommandValidator();

        var result = validator.TestValidate(new CreateOrderCommand
        {
            UserId = 1,
            ShippingAddress = "Some valid address",
            BaseUrl = "http://localhost",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = 1, Quantity = 2 }
            }
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── Reviews ────────────────────────────────────────────────────

    [Fact]
    public void AddReview_WhenRatingIsOutOfRange_ShouldHaveError()
    {
        var validator = new AddReviewCommandValidator();

        var result = validator.TestValidate(new AddReviewCommand
        {
            UserId = 1,
            ProductId = 1,
            Text = "Nice product",
            Rating = 6
        });

        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void AddReview_WhenTextIsTooShort_ShouldHaveError()
    {
        var validator = new AddReviewCommandValidator();

        var result = validator.TestValidate(new AddReviewCommand
        {
            UserId = 1,
            ProductId = 1,
            Text = "ok",
            Rating = 5
        });

        result.ShouldHaveValidationErrorFor(x => x.Text);
    }

    [Fact]
    public void AddReview_WhenFileTypeIsNotAllowed_ShouldHaveError()
    {
        var validator = new AddReviewCommandValidator();

        var result = validator.TestValidate(new AddReviewCommand
        {
            UserId = 1,
            ProductId = 1,
            Text = "Nice product",
            Rating = 5,
            Files = new List<FileUploadDto>
            {
                new()
                {
                    Stream = new MemoryStream(new byte[] { 1 }),
                    FileName = "malware.exe",
                    ContentType = "application/x-msdownload",
                    Length = 1
                }
            }
        });

        result.ShouldHaveValidationErrorFor("Files[0].ContentType");
    }

    [Fact]
    public void EditReview_WhenNothingToUpdate_ShouldHaveError()
    {
        var validator = new EditReviewCommandValidator();

        var result = validator.TestValidate(new EditReviewCommamd
        {
            ReviewId = 1,
            UserId = 1
        });

        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "Nothing to update: provide new text or rating");
    }

    [Fact]
    public void EditReview_WhenRatingIsOutOfRange_ShouldHaveError()
    {
        var validator = new EditReviewCommandValidator();

        var result = validator.TestValidate(new EditReviewCommamd
        {
            ReviewId = 1,
            UserId = 1,
            NewRating = 0
        });

        result.ShouldHaveValidationErrorFor(x => x.NewRating);
    }

    // ── Period filters ─────────────────────────────────────────────

    [Fact]
    public void AdminMostPopular_WhenFirstDayIsLaterThanLastDay_ShouldHaveError()
    {
        var validator = new GetMostPopularProductsForThePeriodCommandValidator();

        var result = validator.TestValidate(new GetMostPopularProductsForThePeriodCommand
        {
            FirstDayOfThePeriod = new DateTime(2026, 2, 1),
            LastDayOfThePeriod = new DateTime(2026, 1, 1)
        });

        result.ShouldHaveValidationErrorFor(x => x.LastDayOfThePeriod);
    }

    [Fact]
    public void Revenue_WhenFirstDayIsLaterThanLastDay_ShouldHaveError()
    {
        var validator = new GetRevenueForThePeriodCommandValidator();

        var result = validator.TestValidate(new GetRevenueForThePeriodCommand
        {
            FirstDayOfThePriod = new DateTime(2026, 2, 1),
            LastDayOfThePriod = new DateTime(2026, 1, 1)
        });

        result.ShouldHaveValidationErrorFor(x => x.LastDayOfThePriod);
    }

    [Fact]
    public void NewOrders_WhenFirstDayIsLaterThanLastDay_ShouldHaveError()
    {
        var validator = new GetNumberOfNewOrdersForThePeriodCommandValidator();

        var result = validator.TestValidate(new GetNumberOfNewOrdersForThePeriodCommand
        {
            FirstDayOfThePriod = new DateTime(2026, 2, 1),
            LastDayOfThePriod = new DateTime(2026, 1, 1)
        });

        result.ShouldHaveValidationErrorFor(x => x.LastDayOfThePriod);
    }

    [Fact]
    public void UserMostPopular_WhenFirstDayIsLaterThanLastDay_ShouldHaveError()
    {
        var validator = new GetMostPopularProductsForThePeriodForUserCommandValidator();

        var result = validator.TestValidate(new GetMostPopularProductsForThePeriodForUserCommand
        {
            FirstDayOfThePeriod = new DateTime(2026, 2, 1),
            LastDayOfThePeriod = new DateTime(2026, 1, 1)
        });

        result.ShouldHaveValidationErrorFor(x => x.LastDayOfThePeriod);
    }

    [Fact]
    public void OrderHistory_WhenFirstDayIsLaterThanLastDay_ShouldHaveError()
    {
        var validator = new GetOrderHistoryQueryValidator();

        var result = validator.TestValidate(new GetOrderHistoryQuery
        {
            UserId = 1,
            FirstDayOfThePeriod = new DateTime(2026, 2, 1),
            LastDayOfThePeriod = new DateTime(2026, 1, 1)
        });

        result.ShouldHaveValidationErrorFor(x => x.LastDayOfThePeriod);
    }

    [Fact]
    public void OrderHistory_WhenPeriodIsNotFullySpecified_ShouldNotHavePeriodError()
    {
        var validator = new GetOrderHistoryQueryValidator();

        var result = validator.TestValidate(new GetOrderHistoryQuery
        {
            UserId = 1,
            FirstDayOfThePeriod = new DateTime(2026, 2, 1)
        });

        result.ShouldNotHaveValidationErrorFor(x => x.LastDayOfThePeriod);
    }

    [Fact]
    public void AdminOrders_WhenFirstDayIsLaterThanLastDay_ShouldHaveError()
    {
        var validator = new GetAllOrderOrFilteredCommandValidator();

        var result = validator.TestValidate(new GetAllOrderOrFilteredCommand
        {
            FirstDayOfThePeriod = new DateTime(2026, 2, 1),
            LastDayOfThePeriod = new DateTime(2026, 1, 1)
        });

        result.ShouldHaveValidationErrorFor(x => x.LastDayOfThePeriod);
    }

    [Fact]
    public void OrderHistory_WhenPageSizeIsTooLarge_ShouldHaveError()
    {
        var validator = new GetOrderHistoryQueryValidator();

        var result = validator.TestValidate(new GetOrderHistoryQuery
        {
            UserId = 1,
            PageSize = 500
        });

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void AdminOrders_WhenPageSizeIsTooLarge_ShouldHaveError()
    {
        var validator = new GetAllOrderOrFilteredCommandValidator();

        var result = validator.TestValidate(new GetAllOrderOrFilteredCommand
        {
            PageSize = 500
        });

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    // ── Product reviews query ──────────────────────────────────────

    [Fact]
    public void ProductReviews_WhenFromDateIsLaterThanToDate_ShouldHaveError()
    {
        var validator = new GetProductReviewsQueryValidator();

        var result = validator.TestValidate(new GetProductReviewsQuery
        {
            ProductId = 1,
            FromDate = new DateTime(2026, 2, 1),
            ToDate = new DateTime(2026, 1, 1)
        });

        result.ShouldHaveValidationErrorFor(x => x.ToDate);
    }

    [Fact]
    public void ProductReviews_WhenRatingIsOutOfRange_ShouldHaveError()
    {
        var validator = new GetProductReviewsQueryValidator();

        var result = validator.TestValidate(new GetProductReviewsQuery
        {
            ProductId = 1,
            Rating = 6
        });

        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void ProductReviews_WhenPageSizeIsZero_ShouldHaveError()
    {
        var validator = new GetProductReviewsQueryValidator();

        var result = validator.TestValidate(new GetProductReviewsQuery
        {
            ProductId = 1,
            PageSize = 0
        });

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void ProductReviews_WhenQueryIsValid_ShouldNotHaveErrors()
    {
        var validator = new GetProductReviewsQueryValidator();

        var result = validator.TestValidate(new GetProductReviewsQuery
        {
            ProductId = 1,
            Rating = 5,
            MinRating = 4,
            FromDate = new DateTime(2026, 1, 1),
            ToDate = new DateTime(2026, 2, 1),
            PageNumber = 1,
            PageSize = 10
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── Same calendar day, different time of day ───────────────────

    [Fact]
    public void AdminMostPopular_WhenSameDayWithDifferentTimes_ShouldNotHaveError()
    {
        var validator = new GetMostPopularProductsForThePeriodCommandValidator();

        var result = validator.TestValidate(new GetMostPopularProductsForThePeriodCommand
        {
            FirstDayOfThePeriod = new DateTime(2026, 1, 1, 18, 0, 0),
            LastDayOfThePeriod = new DateTime(2026, 1, 1, 9, 0, 0)
        });

        result.ShouldNotHaveValidationErrorFor(x => x.LastDayOfThePeriod);
    }

    [Fact]
    public void Revenue_WhenSameDayWithDifferentTimes_ShouldNotHaveError()
    {
        var validator = new GetRevenueForThePeriodCommandValidator();

        var result = validator.TestValidate(new GetRevenueForThePeriodCommand
        {
            FirstDayOfThePriod = new DateTime(2026, 1, 1, 18, 0, 0),
            LastDayOfThePriod = new DateTime(2026, 1, 1, 9, 0, 0)
        });

        result.ShouldNotHaveValidationErrorFor(x => x.LastDayOfThePriod);
    }

    [Fact]
    public void NewOrders_WhenSameDayWithDifferentTimes_ShouldNotHaveError()
    {
        var validator = new GetNumberOfNewOrdersForThePeriodCommandValidator();

        var result = validator.TestValidate(new GetNumberOfNewOrdersForThePeriodCommand
        {
            FirstDayOfThePriod = new DateTime(2026, 1, 1, 18, 0, 0),
            LastDayOfThePriod = new DateTime(2026, 1, 1, 9, 0, 0)
        });

        result.ShouldNotHaveValidationErrorFor(x => x.LastDayOfThePriod);
    }

    [Fact]
    public void UserMostPopular_WhenSameDayWithDifferentTimes_ShouldNotHaveError()
    {
        var validator = new GetMostPopularProductsForThePeriodForUserCommandValidator();

        var result = validator.TestValidate(new GetMostPopularProductsForThePeriodForUserCommand
        {
            FirstDayOfThePeriod = new DateTime(2026, 1, 1, 18, 0, 0),
            LastDayOfThePeriod = new DateTime(2026, 1, 1, 9, 0, 0)
        });

        result.ShouldNotHaveValidationErrorFor(x => x.LastDayOfThePeriod);
    }

    [Fact]
    public void OrderHistory_WhenSameDayWithDifferentTimes_ShouldNotHaveError()
    {
        var validator = new GetOrderHistoryQueryValidator();

        var result = validator.TestValidate(new GetOrderHistoryQuery
        {
            UserId = 1,
            FirstDayOfThePeriod = new DateTime(2026, 1, 1, 18, 0, 0),
            LastDayOfThePeriod = new DateTime(2026, 1, 1, 9, 0, 0)
        });

        result.ShouldNotHaveValidationErrorFor(x => x.LastDayOfThePeriod);
    }

    [Fact]
    public void AdminOrders_WhenSameDayWithDifferentTimes_ShouldNotHaveError()
    {
        var validator = new GetAllOrderOrFilteredCommandValidator();

        var result = validator.TestValidate(new GetAllOrderOrFilteredCommand
        {
            FirstDayOfThePeriod = new DateTime(2026, 1, 1, 18, 0, 0),
            LastDayOfThePeriod = new DateTime(2026, 1, 1, 9, 0, 0)
        });

        result.ShouldNotHaveValidationErrorFor(x => x.LastDayOfThePeriod);
    }

    [Fact]
    public void ProductReviews_WhenSameDayWithDifferentTimes_ShouldNotHaveError()
    {
        var validator = new GetProductReviewsQueryValidator();

        var result = validator.TestValidate(new GetProductReviewsQuery
        {
            ProductId = 1,
            FromDate = new DateTime(2026, 1, 1, 18, 0, 0),
            ToDate = new DateTime(2026, 1, 1, 9, 0, 0)
        });

        result.ShouldNotHaveValidationErrorFor(x => x.ToDate);
    }
}
