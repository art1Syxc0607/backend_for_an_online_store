using Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using DomainValidationException = Domain.Exceptions.ValidationException;

namespace ApplicationTests.Validation;

public class ValidationBehaviorTests
{
    private sealed class TestRequest
    {
        public string Name { get; init; } = string.Empty;
    }

    private sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required");
        }
    }

    private static ValidationBehavior<TestRequest, string> CreateBehavior(
        params IValidator<TestRequest>[] validators)
        => new(
            validators,
            NullLogger<ValidationBehavior<TestRequest, string>>.Instance);

    [Fact]
    public async Task Handle_WhenNoValidatorsRegistered_CallsNext()
    {
        var behavior = CreateBehavior();
        var nextCalled = false;

        RequestHandlerDelegate<string> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult("handler-result");
        };

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.Should().Be("handler-result");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenRequestIsValid_CallsNext()
    {
        var behavior = CreateBehavior(new TestRequestValidator());
        var nextCalled = false;

        RequestHandlerDelegate<string> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult("handler-result");
        };

        var result = await behavior.Handle(
            new TestRequest { Name = "valid" },
            next,
            CancellationToken.None);

        result.Should().Be("handler-result");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenRequestIsInvalid_ThrowsValidationException()
    {
        var behavior = CreateBehavior(new TestRequestValidator());
        var nextCalled = false;

        RequestHandlerDelegate<string> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult("handler-result");
        };

        var act = () => behavior.Handle(new TestRequest(), next, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<DomainValidationException>();

        exception.Which.ErrorCode.Should().Be("VALIDATION_ERROR");
        exception.Which.StatusCode.Should().Be(400);
        exception.Which.Errors.Should().ContainSingle(e =>
            e.Field == "Name" && e.Message == "Name is required");
        nextCalled.Should().BeFalse();
    }
}
