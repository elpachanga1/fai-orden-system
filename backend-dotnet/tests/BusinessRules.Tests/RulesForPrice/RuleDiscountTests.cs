using BusinessRules.RulesForPrice;
using FluentAssertions;
using Xunit;

namespace BusinessRules.Tests.RulesForPrice;

/// <summary>
/// Reglas de descuento:
///   - Descuento del 20% por cada 3 unidades completas
///   - Descuento máximo del 50%
/// Ejemplos:  0-2 uds → 0%  |  3-5 → 20%  |  6-8 → 40%  |  ≥9 → 50% (cap)
/// </summary>
public class RuleDiscountTests
{
    private readonly RuleDiscount _sut = new();

    // ── Sin descuento ────────────────────────────────────────────────────────

    [Fact]
    public void CalculatePrice_MenosDe3Unidades_SinDescuento()
    {
        // 2 uds * $10 = $20, 0 descuento
        float result = _sut.CalculatePrice(2, 10);

        result.Should().BeApproximately(20f, 0.001f);
    }

    [Fact]
    public void CalculatePrice_CeroUnidades_RetornaCero()
    {
        float result = _sut.CalculatePrice(0, 10);

        result.Should().Be(0f);
    }

    // ── 20% de descuento (primer escalón: 3 uds) ─────────────────────────────

    [Fact]
    public void CalculatePrice_3Unidades_Aplica20PorCientoDescuento()
    {
        // 3 * $10 = $30, descuento 20% → $24
        float result = _sut.CalculatePrice(3, 10);

        result.Should().BeApproximately(24f, 0.001f);
    }

    [Fact]
    public void CalculatePrice_5Unidades_AplicaDescuentoDelPrimerEscalon()
    {
        // (int)5/3 = 1 → 20%  ;  5 * $10 = $50, 20% → $40
        float result = _sut.CalculatePrice(5, 10);

        result.Should().BeApproximately(40f, 0.001f);
    }

    // ── 40% de descuento (segundo escalón: 6 uds) ────────────────────────────

    [Fact]
    public void CalculatePrice_6Unidades_Aplica40PorCientoDescuento()
    {
        // 6 * $10 = $60, descuento 40% → $36
        float result = _sut.CalculatePrice(6, 10);

        result.Should().BeApproximately(36f, 0.001f);
    }

    // ── 50% de descuento (tope máximo) ───────────────────────────────────────

    [Fact]
    public void CalculatePrice_9Unidades_CapadoAl50PorCientoDescuento()
    {
        // (int)9/3 = 3 → 60%, pero cap = 50%  ;  9 * $10 = $90, 50% → $45
        float result = _sut.CalculatePrice(9, 10);

        result.Should().BeApproximately(45f, 0.001f);
    }

    [Fact]
    public void CalculatePrice_MuchasUnidades_NuncaSupera50PorCientoDescuento()
    {
        // 30 uds * $100 = $3000, cap 50% → $1500
        float result = _sut.CalculatePrice(30, 100);

        result.Should().BeApproximately(1500f, 0.01f);
    }

    // ── Theory: varios escenarios ─────────────────────────────────────────────

    [Theory]
    [InlineData(1,  10f,   10f)]   // 0%
    [InlineData(3,  10f,   24f)]   // 20%
    [InlineData(6,  10f,   36f)]   // 40%
    [InlineData(9,  10f,   45f)]   // 50% (cap)
    [InlineData(12, 10f,   60f)]   // 50% (cap)
    public void CalculatePrice_EscenariosClave_RetornaPrecioEsperado(float qty, float price, float expected)
    {
        float result = _sut.CalculatePrice(qty, price);

        result.Should().BeApproximately(expected, 0.001f);
    }
}
