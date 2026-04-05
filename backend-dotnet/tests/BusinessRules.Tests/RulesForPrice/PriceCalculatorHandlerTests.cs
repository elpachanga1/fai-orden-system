using BusinessRules.RulesForPrice.Handlers;
using FluentAssertions;
using Xunit;

namespace BusinessRules.Tests.RulesForPrice;

public class PriceCalculatorHandlerTests
{
    private readonly PriceCalculatorHandler _sut = PriceCalculatorHandler.GetInstance();

    // ── GetInstance (singleton) ───────────────────────────────────────────────

    [Fact]
    public void GetInstance_SiempreRetornaLaMismaInstancia()
    {
        var instancia1 = PriceCalculatorHandler.GetInstance();
        var instancia2 = PriceCalculatorHandler.GetInstance();

        instancia1.Should().BeSameAs(instancia2);
    }

    // ── SKU "EA" → RuleNormal (precio = qty * price) ─────────────────────────

    [Fact]
    public void CalculateItemPrice_SkuEA_UsaRuleNormal()
    {
        float result = _sut.CalculateItemPrice("EA-001", 3, 10);

        result.Should().BeApproximately(30f, 0.001f);
    }

    [Fact]
    public void CalculateItemPrice_SkuEAMinusculas_NoCoincide_LanzaExcepcion()
    {
        // El prefijo es case-sensitive: "ea" no empieza con "EA"
        Action act = () => _sut.CalculateItemPrice("ea-001", 1, 10);

        act.Should().Throw<ArgumentException>().WithMessage("*SKU Not Implemented*");
    }

    // ── SKU "WE" → RuleWeighBased (precio = qty * price * 1000) ─────────────

    [Fact]
    public void CalculateItemPrice_SkuWE_UsaRuleWeighBased()
    {
        // 2 kg * $5 * 1000 = $10 000
        float result = _sut.CalculateItemPrice("WE-001", 2, 5);

        result.Should().BeApproximately(10_000f, 0.001f);
    }

    // ── SKU "SP" → RuleDiscount (20% por cada 3 uds, cap 50%) ───────────────

    [Fact]
    public void CalculateItemPrice_SkuSP_UsaRuleDiscount()
    {
        // 3 uds * $10 = $30, descuento 20% → $24
        float result = _sut.CalculateItemPrice("SP-001", 3, 10);

        result.Should().BeApproximately(24f, 0.001f);
    }

    [Fact]
    public void CalculateItemPrice_SkuSP_DescuentoCapadoAl50Por100()
    {
        // 9 uds * $10 = $90, cap 50% → $45
        float result = _sut.CalculateItemPrice("SP-001", 9, 10);

        result.Should().BeApproximately(45f, 0.001f);
    }

    // ── SKU desconocido → ArgumentException ──────────────────────────────────

    [Fact]
    public void CalculateItemPrice_SkuDesconocido_LanzaArgumentException()
    {
        Action act = () => _sut.CalculateItemPrice("XX-001", 1, 10);

        act.Should().Throw<ArgumentException>().WithMessage("*SKU Not Implemented*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("ZZ-999")]
    [InlineData("123")]
    public void CalculateItemPrice_SkuInvalido_SiempreLanzaArgumentException(string sku)
    {
        Action act = () => _sut.CalculateItemPrice(sku, 1, 10);

        act.Should().Throw<ArgumentException>();
    }
}
