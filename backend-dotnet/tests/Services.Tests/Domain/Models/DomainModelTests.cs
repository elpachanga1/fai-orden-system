using FluentAssertions;
using Services.Domain.Models;
using Xunit;

namespace Services.Tests.Domain.Models;

public class ProductModelTests
{
    [Fact]
    public void HasUnits_PositiveStock_ReturnsTrue()
    {
        var product = new Product { Sku = "A", Name = "A", AvailableUnits = 5, UnitPrice = 1f };
        product.HasUnits().Should().BeTrue();
    }

    [Fact]
    public void HasUnits_ZeroStock_ReturnsFalse()
    {
        var product = new Product { Sku = "A", Name = "A", AvailableUnits = 0, UnitPrice = 1f };
        product.HasUnits().Should().BeFalse();
    }

    [Fact]
    public void DiscountUnits_SubtractsCorrectly()
    {
        var product = new Product { Sku = "A", Name = "A", AvailableUnits = 10, UnitPrice = 1f };
        product.DiscountUnits(3);
        product.AvailableUnits.Should().Be(7);
    }

    [Fact]
    public void DiscountUnits_FullQuantity_LeavesZero()
    {
        var product = new Product { Sku = "A", Name = "A", AvailableUnits = 5, UnitPrice = 1f };
        product.DiscountUnits(5);
        product.AvailableUnits.Should().Be(0);
    }
}

public class ShoppingCartModelTests
{
    [Fact]
    public void CalculateTotal_EmptyItems_ReturnsZero()
    {
        var cart = new ShoppingCart();
        cart.CalculateTotal().Should().Be(0f);
    }

    [Fact]
    public void CalculateTotal_WithItems_ReturnsSumOfTotalPrices()
    {
        var cart = new ShoppingCart();
        cart.items.Add(new Item { TotalPrice = 10f });
        cart.items.Add(new Item { TotalPrice = 25.5f });
        cart.items.Add(new Item { TotalPrice = 4.5f });

        cart.CalculateTotal().Should().BeApproximately(40f, 0.001f);
    }

    [Fact]
    public void CalculateTotal_SingleItem_ReturnsThatItemPrice()
    {
        var cart = new ShoppingCart();
        cart.items.Add(new Item { TotalPrice = 99.99f });

        cart.CalculateTotal().Should().BeApproximately(99.99f, 0.001f);
    }
}
