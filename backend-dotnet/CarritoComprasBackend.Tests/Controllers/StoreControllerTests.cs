using CarritoComprasBackend.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Domain.Models;
using Services.Domain.Services;
using ShoppingCartBackEnd.Entities.Models.InputModels;
using Validations;
using Validations.Interface;
using Validations.Model;
using Xunit;

namespace CarritoComprasBackend.Tests.Controllers;

public class StoreControllerTests
{
    private readonly Mock<StoreService> _mockStoreService;
    private readonly Mock<ICreatorFactory> _mockCreatorFactory;
    private readonly Mock<IHandler> _mockHandler;
    private readonly StoreController _sut;

    public StoreControllerTests()
    {
        _mockStoreService = new Mock<StoreService>(
            (ProductService?)null!, (UserService?)null!, (ItemService?)null!);
        _mockCreatorFactory = new Mock<ICreatorFactory>();
        _mockHandler = new Mock<IHandler>();
        _mockCreatorFactory.Setup(f => f.CreateChain()).Returns(_mockHandler.Object);

        _sut = new StoreController(_mockStoreService.Object, _mockCreatorFactory.Object);
    }

    // ── GetProductById ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProductById_ProductoEncontrado_RetornaOkConProducto()
    {
        var producto = new Product { Id = 1, Sku = "EA-001", Name = "Manzana" };
        _mockStoreService.Setup(s => s.GetProductById(1)).ReturnsAsync(producto);

        var result = await _sut.GetProductById(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(producto);
    }

    [Fact]
    public async Task GetProductById_ProductoNoEncontrado_RetornaNotFound()
    {
        _mockStoreService.Setup(s => s.GetProductById(It.IsAny<int>()))
                         .ReturnsAsync((Product?)null);

        var result = await _sut.GetProductById(99);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetProductById_ExcepcionEnServicio_Retorna500()
    {
        _mockStoreService.Setup(s => s.GetProductById(It.IsAny<int>()))
                         .ThrowsAsync(new Exception("DB error"));

        var result = await _sut.GetProductById(1);

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    // ── GetAllProducts ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_RetornaTodosLosProductos()
    {
        var productos = new List<Product>
        {
            new Product { Id = 1, Sku = "EA-001", Name = "Manzana" },
            new Product { Id = 2, Sku = "SP-001", Name = "Naranja" }
        };
        _mockStoreService.Setup(s => s.GetAllProducts()).ReturnsAsync(productos);

        var result = await _sut.GetProducts();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(productos);
    }

    [Fact]
    public async Task GetProducts_ExcepcionEnServicio_Retorna500()
    {
        _mockStoreService.Setup(s => s.GetAllProducts())
                         .ThrowsAsync(new Exception("Timeout"));

        var result = await _sut.GetProducts();

        result.Should().BeOfType<ObjectResult>()
              .Which.StatusCode.Should().Be(500);
    }

    // ── Add (AddProduct) ──────────────────────────────────────────────────────

    [Fact]
    public async Task Add_ProductoAgregadoExitosamente_RetornaOk()
    {
        _mockStoreService
            .Setup(s => s.AddProduct(It.IsAny<string>(), It.IsAny<string>(),
                                     It.IsAny<string?>(), It.IsAny<int>(),
                                     It.IsAny<float>(), It.IsAny<string?>()))
            .ReturnsAsync(true);

        var input = new ProductInputModel
        {
            Sku = "EA-001", Name = "Manzana",
            Description = null, AvailableUnits = 10,
            UnitPrice = 1.5f, Image = null
        };

        var result = await _sut.Add(input);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Add_ServicioDevuelveFalse_Retorna500()
    {
        _mockStoreService
            .Setup(s => s.AddProduct(It.IsAny<string>(), It.IsAny<string>(),
                                     It.IsAny<string?>(), It.IsAny<int>(),
                                     It.IsAny<float>(), It.IsAny<string?>()))
            .ReturnsAsync(false);

        var input = new ProductInputModel { Sku = "EA-001", Name = "Manzana" };
        var result = await _sut.Add(input);

        result.Should().BeOfType<ObjectResult>()
              .Which.StatusCode.Should().Be(500);
    }

    // ── GetItemsByProductId ───────────────────────────────────────────────────

    [Fact]
    public async Task GetItemsByProductId_ItemsEncontrados_RetornaOk()
    {
        var items = new List<Item>
        {
            new Item { Id = 1, IdProduct = 5, Quantity = 2 }
        };
        _mockStoreService.Setup(s => s.GetItemsByProductId(5)).ReturnsAsync(items);

        var result = await _sut.GetItemsByProductId(5);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(items);
    }

    [Fact]
    public async Task GetItemsByProductId_ItemsNoEncontrados_RetornaNotFound()
    {
        _mockStoreService.Setup(s => s.GetItemsByProductId(It.IsAny<int>()))
                         .ReturnsAsync((IEnumerable<Item>?)null);

        var result = await _sut.GetItemsByProductId(404);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── GetAllItems ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllItems_RetornaTodosLosItems()
    {
        var items = new List<Item>
        {
            new Item { Id = 1, IdProduct = 1, Quantity = 5 },
            new Item { Id = 2, IdProduct = 2, Quantity = 3 }
        };
        _mockStoreService.Setup(s => s.GetAllItems()).ReturnsAsync(items);

        var result = await _sut.GetAllItems();

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(items);
    }

    // ── AddProductToShoppingCart ──────────────────────────────────────────────

    [Fact]
    public async Task AddProductToShoppingCart_ProductoAgregado_RetornaOk()
    {
        _mockStoreService.Setup(s => s.AddProductToShoppingCart("user1", 1, 2))
                         .ReturnsAsync(true);

        var result = await _sut.AddProductToShoppingCart("user1", 1, 2);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task AddProductToShoppingCart_ExcepcionEnServicio_Retorna500()
    {
        _mockStoreService.Setup(s => s.AddProductToShoppingCart(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                         .ThrowsAsync(new Exception("Cart error"));

        var result = await _sut.AddProductToShoppingCart("user1", 1, 2);

        result.Should().BeOfType<ObjectResult>()
              .Which.StatusCode.Should().Be(500);
    }

    // ── DeleteProductFromShoppingCart ─────────────────────────────────────────

    [Fact]
    public async Task DeleteProductFromShoppingCart_ProductoEliminado_RetornaOk()
    {
        _mockStoreService.Setup(s => s.DeleteProductFromShoppingCart("user1", 10))
                         .ReturnsAsync(true);

        var result = await _sut.DeleteProductFromShoppingCartAsync("user1", 10);

        result.Should().BeOfType<OkResult>();
    }

    // ── EmptyShoppingCart ─────────────────────────────────────────────────────

    [Fact]
    public async Task EmptyShoppingCart_CarritoVaciado_RetornaOk()
    {
        _mockStoreService.Setup(s => s.EmptyShoppingCart("user1")).ReturnsAsync(true);

        var result = await _sut.EmptyShoppingCartAsync("user1");

        result.Should().BeOfType<OkResult>();
    }

    // ── CompleteCartTransaction ───────────────────────────────────────────────

    [Fact]
    public async Task CompleteCartTransaction_ExcepcionEnServicio_Retorna500()
    {
        _mockStoreService.Setup(s => s.CompleteshoppingCart(It.IsAny<string>()))
                         .ThrowsAsync(new Exception("Transaction failed"));

        var result = await _sut.CompleteCartTransaction("user-error");

        result.Should().BeOfType<ObjectResult>()
              .Which.StatusCode.Should().Be(500);
    }

    // ── GetTotalSales ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTotalSales_RetornaTotalDeVentas()
    {
        _mockStoreService.Setup(s => s.GetTotalSales()).ReturnsAsync(1500.75f);

        var result = await _sut.GetTotalSales();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(1500.75f);
    }

    [Fact]
    public async Task GetTotalSales_ExcepcionEnServicio_Retorna500()
    {
        _mockStoreService.Setup(s => s.GetTotalSales())
                         .ThrowsAsync(new Exception("Sales error"));

        var result = await _sut.GetTotalSales();

        result.Should().BeOfType<ObjectResult>()
              .Which.StatusCode.Should().Be(500);
    }
}
