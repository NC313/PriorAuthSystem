using FluentAssertions;
using PriorAuthSystem.Domain.ValueObjects;

namespace PriorAuthSystem.Tests.Domain.ValueObjects;

public class CptCodeTests
{
    [Fact]
    public void Constructor_WithValidFiveCharCode_Succeeds()
    {
        var act = () => new CptCode("96413", "Chemotherapy infusion");

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithEmptyCode_ThrowsArgumentException()
    {
        var act = () => new CptCode("", "description");

        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Constructor_WithCodeShorterThanFive_ThrowsArgumentException()
    {
        var act = () => new CptCode("9641", "description");

        act.Should().Throw<ArgumentException>().WithMessage("*5*");
    }

    [Fact]
    public void Constructor_WithCodeLongerThanFive_ThrowsArgumentException()
    {
        var act = () => new CptCode("964130", "description");

        act.Should().Throw<ArgumentException>().WithMessage("*5*");
    }

    [Fact]
    public void Constructor_DefaultsRequiresPriorAuthToTrue()
    {
        var cpt = new CptCode("96413", "Chemotherapy infusion");

        cpt.RequiresPriorAuth.Should().BeTrue();
    }

    [Fact]
    public void Equality_TwoCodesWithSameCode_AreEqual()
    {
        var a = new CptCode("96413", "desc1");
        var b = new CptCode("96413", "desc2");

        a.Should().Be(b);
    }

    [Fact]
    public void ToString_ReturnsCodeAndDescription()
    {
        var cpt = new CptCode("96413", "Chemotherapy");

        cpt.ToString().Should().Be("96413 - Chemotherapy");
    }
}
