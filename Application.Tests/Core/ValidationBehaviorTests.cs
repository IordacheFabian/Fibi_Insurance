using FluentAssertions;
using FluentValidation;

namespace Application.Tests.Core;

public class ValidationBehaviorTests
{
    private record TestRequest(string Value);

    [Fact]
    public async Task Handle_ShouldInvokeNext_WhenValidatorsAbsent()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(Array.Empty<IValidator<TestRequest>>());

        var result = await behavior.Handle(
            new TestRequest("ok"),
            _ => Task.FromResult("next"),
            CancellationToken.None);

        result.Should().Be("next");
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(x => x.Value).NotEmpty();

        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });

        var act = async () => await behavior.Handle(
            new TestRequest(string.Empty),
            _ => Task.FromResult("noop"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
