using BusinessRules.RulesForPrice;
using FluentAssertions;
using Xunit;

namespace BusinessRules.Tests.RulesForPrice;

public class RuleNormalTests
{
    private readonly RuleNormal _sut = new();

    [Fact]
    public void CalculatePrice_MultiplicaQuantityPorPrice()
    {
        float result = _sut.CalculatePrice(2, 10);

        result.Should().BeApproximately(20f, 0.001f);
    }

    [Fact]
    public void CalculatePrice_CuandoQuantityEsCero_RetornaCero()
    {
        float result = _sut.CalculatePrice(0, 10);

        result.Should().Be(0f);
    }

    [Fact]
    public void CalculatePrice_CuandoPriceEsCero_RetornaCero()
    {
        float result = _sut.CalculatePrice(5, 0);

        result.Should().Be(0f);
    }

    [Theory]
    [InlineData(1, 5, 5)]
    [InlineData(3, 7, 21)]
    [InlineData(10, 2.5f, 25)]
    public void CalculatePrice_VariosEscenarios_RetornaProductoEsperado(float qty, float price, float expected)
    {
        float result = _sut.CalculatePrice(qty, price);

        result.Should().BeApproximately(expected, 0.001f);
    }
}
