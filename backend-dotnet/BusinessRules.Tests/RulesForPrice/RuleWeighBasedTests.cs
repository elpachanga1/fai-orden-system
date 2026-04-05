using BusinessRules.RulesForPrice;
using FluentAssertions;
using Xunit;

namespace BusinessRules.Tests.RulesForPrice;

public class RuleWeighBasedTests
{
    private readonly RuleWeighBased _sut = new();

    [Fact]
    public void CalculatePrice_MultiplicaQuantityPorPriceY1000()
    {
        float result = _sut.CalculatePrice(2, 10);

        result.Should().BeApproximately(20_000f, 0.001f);
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

    [Fact]
    public void CalculatePrice_ConFraccion_AplicaFactorDe1000()
    {
        // 0.5 kg * precio 4 => 0.5 * 4 * 1000 = 2000
        float result = _sut.CalculatePrice(0.5f, 4f);

        result.Should().BeApproximately(2_000f, 0.01f);
    }

    [Theory]
    [InlineData(1, 1, 1000)]
    [InlineData(3, 2, 6000)]
    [InlineData(0.25f, 8, 2000)]
    public void CalculatePrice_VariosEscenarios_AplicaFactor1000(float qty, float price, float expected)
    {
        float result = _sut.CalculatePrice(qty, price);

        result.Should().BeApproximately(expected, 0.01f);
    }
}
