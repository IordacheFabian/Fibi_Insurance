using Application.Core.util;
using FluentAssertions;

namespace Application.Tests.Core;

public class IdentificationMaskerTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    public void Mask_ShouldReturnEmpty_ForNullOrEmpty(string? input, string expected)
    {
        IdentificationMasker.Mask(input!).Should().Be(expected);
    }

    [Fact]
    public void Mask_ShouldReturnOriginal_WhenLengthLessOrEqualVisible()
    {
        IdentificationMasker.Mask("123", 5).Should().Be("123");
    }

    [Fact]
    public void Mask_ShouldMaskAllButLastFourCharacters()
    {
        IdentificationMasker.Mask("1234567890").Should().Be("******7890");
    }

    [Fact]
    public void Mask_ShouldRespectVisibleCharactersParameter()
    {
        IdentificationMasker.Mask("ABCDEFGH", 2).Should().Be("******GH");
    }
}
