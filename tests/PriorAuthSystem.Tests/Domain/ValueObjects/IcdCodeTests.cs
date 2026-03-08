using FluentAssertions;
using PriorAuthSystem.Domain.ValueObjects;

namespace PriorAuthSystem.Tests.Domain.ValueObjects;

public class IcdCodeTests
{
    [Theory]
    [InlineData("Z51", "Short valid")]
    [InlineData("Z51.11", "Standard code")]
    [InlineData("A001234", "Max length 7")]
    public void Constructor_WithValidCode_Succeeds(string code, string _)
    {
        var act = () => new IcdCode(code, "description");

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_NormalizesCodeToUppercase()
    {
        var icd = new IcdCode("z51.11", "description");

        icd.Code.Should().Be("Z51.11");
    }

    [Fact]
    public void Constructor_WithEmptyCode_ThrowsArgumentException()
    {
        var act = () => new IcdCode("", "description");

        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Constructor_WithCodeTooShort_ThrowsArgumentException()
    {
        var act = () => new IcdCode("AB", "description");

        act.Should().Throw<ArgumentException>().WithMessage("*3*7*");
    }

    [Fact]
    public void Constructor_WithCodeTooLong_ThrowsArgumentException()
    {
        var act = () => new IcdCode("ABCDEFGH", "description"); // 8 chars

        act.Should().Throw<ArgumentException>().WithMessage("*3*7*");
    }

    [Fact]
    public void Equality_TwoCodesWithSameCode_AreEqual()
    {
        var a = new IcdCode("Z51.11", "desc1");
        var b = new IcdCode("Z51.11", "desc2");

        a.Should().Be(b);
    }

    [Fact]
    public void ToString_ReturnsCodeAndDescription()
    {
        var icd = new IcdCode("Z51.11", "Chemotherapy");

        icd.ToString().Should().Be("Z51.11 - Chemotherapy");
    }
}
