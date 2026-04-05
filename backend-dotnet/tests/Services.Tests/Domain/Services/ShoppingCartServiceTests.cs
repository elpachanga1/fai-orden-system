using DataRepository.Repositories;
using FluentAssertions;
using Moq;
using Services.Domain.Services;
using Xunit;

namespace Services.Tests.Domain.Services;

public class ShoppingCartServiceTests
{
    private readonly IMapper _mapper = TestHelpers.CreateMapper();

    private ShoppingCartService BuildSut(
        Mock<IRepository<DataRepository.Models.ShoppingCart>> cartRepo,
        Mock<IRepository<DataRepository.Models.Item>> itemRepo,
        Mock<IRepository<DataRepository.Models.Product>> productRepo)
    {
        var productSvc = new ProductService(_mapper, productRepo.Object);
        var itemSvc = new ItemService(_mapper, itemRepo.Object, productSvc);
        return new ShoppingCartService(_mapper, cartRepo.Object, itemSvc);
    }

    private static DataRepository.Models.ShoppingCart ActiveCart(string userId = "user-1") =>
        new() { Id = 1, IdUser = userId, IsCompleted = false, CreationDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow };

    // ---------------------------------------------------------------
    // AddProductToShoppingCart
    // ---------------------------------------------------------------

    [Fact]
    public async Task AddProductToShoppingCart_ExistingActiveCart_UsesExistingCartAndCreatesItem()
    {
        var cartRepo = new Mock<IRepository<DataRepository.Models.ShoppingCart>>();
        var itemRepo = new Mock<IRepository<DataRepository.Models.Item>>();
        var productRepo = new Mock<IRepository<DataRepository.Models.Product>>();

        cartRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DataRepository.Models.ShoppingCart> { ActiveCart() });
        cartRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(ActiveCart());

        itemRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DataRepository.Models.Item>());
        itemRepo.Setup(r => r.AddAsync(It.IsAny<DataRepository.Models.Item>())).Returns(Task.CompletedTask);
        itemRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        productRepo.Setup(r => r.FindByIdAsync(10)).ReturnsAsync(new DataRepository.Models.Product
        {
            Id = 10, Sku = "EA-001", Name = "Glass", UnitPrice = 5f, AvailableUnits = 10
        });

        var svc = BuildSut(cartRepo, itemRepo, productRepo);
        var result = await svc.AddProductToShoppingCart("user-1", 10, 2);

        // result=false because the cart already existed (no new cart created)
        result.Should().BeFalse();
        cartRepo.Verify(r => r.AddAsync(It.IsAny<DataRepository.Models.ShoppingCart>()), Times.Never);
    }

    [Fact]
    public async Task AddProductToShoppingCart_NoExistingCart_CreatesNewCart()
    {
        var cartRepo = new Mock<IRepository<DataRepository.Models.ShoppingCart>>();
        var itemRepo = new Mock<IRepository<DataRepository.Models.Item>>();
        var productRepo = new Mock<IRepository<DataRepository.Models.Product>>();

        var createdCart = ActiveCart();
        cartRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DataRepository.Models.ShoppingCart>());
        cartRepo.Setup(r => r.AddAsync(It.IsAny<DataRepository.Models.ShoppingCart>()))
            .Callback<DataRepository.Models.ShoppingCart>(c => c.Id = 1)
            .Returns(Task.CompletedTask);
        cartRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);
        cartRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(createdCart);

        itemRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DataRepository.Models.Item>());
        itemRepo.Setup(r => r.AddAsync(It.IsAny<DataRepository.Models.Item>())).Returns(Task.CompletedTask);
        itemRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        productRepo.Setup(r => r.FindByIdAsync(10)).ReturnsAsync(new DataRepository.Models.Product
        {
            Id = 10, Sku = "EA-001", Name = "Glass", UnitPrice = 5f, AvailableUnits = 10
        });

        var svc = BuildSut(cartRepo, itemRepo, productRepo);
        var result = await svc.AddProductToShoppingCart("user-1", 10, 2);

        result.Should().BeTrue();
        cartRepo.Verify(r => r.AddAsync(It.IsAny<DataRepository.Models.ShoppingCart>()), Times.Once);
        cartRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    // ---------------------------------------------------------------
    // CompleteShoppingCart
    // ---------------------------------------------------------------

    [Fact]
    public async Task CompleteShoppingCart_ActiveCart_MarksAsCompleted()
    {
        var cartRepo = new Mock<IRepository<DataRepository.Models.ShoppingCart>>();
        var itemRepo = new Mock<IRepository<DataRepository.Models.Item>>();
        var productRepo = new Mock<IRepository<DataRepository.Models.Product>>();

        var cart = ActiveCart();
        cartRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DataRepository.Models.ShoppingCart> { cart });
        cartRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var svc = BuildSut(cartRepo, itemRepo, productRepo);
        await svc.CompleteShoppingCart("user-1");

        cart.IsCompleted.Should().BeTrue();
        cartRepo.Verify(r => r.Update(It.IsAny<DataRepository.Models.ShoppingCart>()), Times.Once);
        cartRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task CompleteShoppingCart_NoActiveCart_DoesNothing()
    {
        var cartRepo = new Mock<IRepository<DataRepository.Models.ShoppingCart>>();
        var itemRepo = new Mock<IRepository<DataRepository.Models.Item>>();
        var productRepo = new Mock<IRepository<DataRepository.Models.Product>>();

        cartRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DataRepository.Models.ShoppingCart>());

        var svc = BuildSut(cartRepo, itemRepo, productRepo);
        await svc.CompleteShoppingCart("user-1");

        cartRepo.Verify(r => r.Update(It.IsAny<DataRepository.Models.ShoppingCart>()), Times.Never);
        cartRepo.Verify(r => r.SaveAsync(), Times.Never);
    }

    // ---------------------------------------------------------------
    // DeleteProductFromShoppingCart
    // ---------------------------------------------------------------

    [Fact]
    public async Task DeleteProductFromShoppingCart_ActiveCart_DeletesItem()
    {
        var cartRepo = new Mock<IRepository<DataRepository.Models.ShoppingCart>>();
        var itemRepo = new Mock<IRepository<DataRepository.Models.Item>>();
        var productRepo = new Mock<IRepository<DataRepository.Models.Product>>();

        cartRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DataRepository.Models.ShoppingCart> { ActiveCart() });

        var item = new DataRepository.Models.Item { Id = 5 };
        itemRepo.Setup(r => r.FindByIdAsync(5)).ReturnsAsync(item);
        itemRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var svc = BuildSut(cartRepo, itemRepo, productRepo);
        var result = await svc.DeleteProductFromShoppingCart("user-1", 5);

        result.Should().BeTrue();
        itemRepo.Verify(r => r.Remove(item), Times.Once);
    }

    [Fact]
    public async Task DeleteProductFromShoppingCart_NoActiveCart_ReturnsFalse()
    {
        var cartRepo = new Mock<IRepository<DataRepository.Models.ShoppingCart>>();
        var itemRepo = new Mock<IRepository<DataRepository.Models.Item>>();
        var productRepo = new Mock<IRepository<DataRepository.Models.Product>>();

        cartRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DataRepository.Models.ShoppingCart>());

        var svc = BuildSut(cartRepo, itemRepo, productRepo);
        var result = await svc.DeleteProductFromShoppingCart("user-1", 5);

        result.Should().BeFalse();
        itemRepo.Verify(r => r.Remove(It.IsAny<DataRepository.Models.Item>()), Times.Never);
    }

    // ---------------------------------------------------------------
    // EmptyShoppingCart
    // ---------------------------------------------------------------

    [Fact]
    public async Task EmptyShoppingCart_ActiveCart_DeletesAllItems()
    {
        var cartRepo = new Mock<IRepository<DataRepository.Models.ShoppingCart>>();
        var itemRepo = new Mock<IRepository<DataRepository.Models.Item>>();
        var productRepo = new Mock<IRepository<DataRepository.Models.Product>>();

        cartRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DataRepository.Models.ShoppingCart> { ActiveCart() });

        var items = new List<DataRepository.Models.Item> { new() { Id = 1 }, new() { Id = 2 } };
        itemRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(items);
        itemRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var svc = BuildSut(cartRepo, itemRepo, productRepo);
        await svc.EmptyShoppingCart("user-1");

        itemRepo.Verify(r => r.Remove(It.IsAny<DataRepository.Models.Item>()), Times.Exactly(2));
        itemRepo.Verify(r => r.SaveAsync(), Times.Once);
    }
}
