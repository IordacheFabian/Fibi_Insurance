using Application.Core;
using FluentAssertions;

namespace Application.Tests.Core;

public class AppExceptionTests
{
    [Fact]
    public void AppException_ShouldExposeProvidedValues()
    {
        var exception = new AppException(500, "Something broke", "stack");

        exception.StatusCode.Should().Be(500);
        exception.Message.Should().Be("Something broke");
        exception.Details.Should().Be("stack");
    }

    [Fact]
    public void BadRequestException_ShouldKeepMessage()
    {
        var exception = new BadRequestException("bad");

        exception.Message.Should().Be("bad");
    }

    [Fact]
    public void NotFoundException_ShouldKeepMessage()
    {
        var exception = new NotFoundException("missing");

        exception.Message.Should().Be("missing");
    }
}
