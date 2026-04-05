using DataRepository.Repositories;
using FluentAssertions;
using Moq;
using Services.Domain.Services;
using Xunit;

namespace Services.Tests.Domain.Services;

public class ItemServiceTests
{
    private readonly IMapper _mapper = TestHelpers.CreateMapper();

    private (Mock<IRepository<DataRepository.Models.Item>>, Mock<IRepository<DataRepository.Models.Product>>) Mocks() =>
        (new Mock<IRepository<DataRepository.Models.Item>>(),
         new Mock<IRepository<DataRepository.Models.Product>>());

    private ItemService BuildSut(
        Mock<IRepository<DataRepository.Models.Item>> itemRepo,
        Mock<IRepository<DataRepository.Models.Product>> productRepo)
    {
        var productSvc = new ProductService(_mapper, productRepo.Object);
        return new ItemService(_mapper, itemRepo.Object, productSvc);
    }

    // ---------------------------------------------------------------
    // DeleteItem
    // ---------------------------------------------------------------

    [Fact]
    public async Task DeleteItem_ExistingItem_RemovesAndReturnsTrue()
    {
        var (itemRepo, productRepo) = Mocks();
        var item = new DataRepository.Models.Item { Id = 1, IdProduct = 10, IdShoppingCart = 1 };
        itemRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(item);
        itemRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var svc = BuildSut(itemRepo, productRepo);
        var result = await svc.DeleteItem(1);

        result.Should().BeTrue();
        itemRepo.Verify(r => r.Remove(item), Times.Once);
        itemRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteItem_NonExistingItem_ReturnsFalse()
    {
        var (itemRepo, productRepo) = Mocks();
        itemRepo.Setup(r => r.FindByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((DataRepository.Models.Item?)null);

        var svc = BuildSut(itemRepo, productRepo);
        var result = await svc.DeleteItem(999);

        result.Should().BeFalse();
        itemRepo.Verify(r => r.Remove(It.IsAny<DataRepository.Models.Item>()), Times.Never);
    }

    // ---------------------------------------------------------------
    // DeleteItems
    // ---------------------------------------------------------------

    [Fact]
    public async Task DeleteItems_WithItems_RemovesAll()
    {
        var (itemRepo, productRepo) = Mocks();
        var items = new List<DataRepository.Models.Item>
        {
            new() { Id = 1 }, new() { Id = 2 }
        };
        itemRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(items);
        itemRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var svc = BuildSut(itemRepo, productRepo);
        await svc.DeleteItems();

        itemRepo.Verify(r => r.Remove(It.IsAny<DataRepository.Models.Item>()), Times.Exactly(2));
        itemRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    // ---------------------------------------------------------------
    // CreateItem
    // ---------------------------------------------------------------

    [Fact]
    public async Task CreateItem_NewItem_ProductExists_AddsItemAndReturnsTrue()
    {
        var (itemRepo, productRepo) = Mocks();
        itemRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<DataRepository.Models.Item>());
        itemRepo.Setup(r => r.AddAsync(It.IsAny<DataRepository.Models.Item>()))
            .Returns(Task.CompletedTask);
        itemRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);
        productRepo.Setup(r => r.FindByIdAsync(10))
            .ReturnsAsync(new DataRepository.Models.Product
            {
                Id = 10, Sku = "EA-001", Name = "Glass", UnitPrice = 5f, AvailableUnits = 100
            });

        var svc = BuildSut(itemRepo, productRepo);
        var result = await svc.CreateItem(1, 10, 2);

        result.Should().BeTrue();
        itemRepo.Verify(r => r.AddAsync(It.IsAny<DataRepository.Models.Item>()), Times.Once);
        itemRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateItem_ProductNotFound_ReturnsFalse()
    {
        var (itemRepo, productRepo) = Mocks();
        itemRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<DataRepository.Models.Item>());
        productRepo.Setup(r => r.FindByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((DataRepository.Models.Product?)null);

        var svc = BuildSut(itemRepo, productRepo);
        var result = await svc.CreateItem(1, 99, 1);

        result.Should().BeFalse();
        itemRepo.Verify(r => r.AddAsync(It.IsAny<DataRepository.Models.Item>()), Times.Never);
    }

    [Fact]
    public async Task CreateItem_ExistingActiveItem_UpdatesQuantityAndReturnsTrue()
    {
        var (itemRepo, productRepo) = Mocks();
        var existing = new DataRepository.Models.Item
        {
            Id = 1, IdProduct = 10, IdShoppingCart = 1, Quantity = 3, IsDeleted = false, TotalPrice = 15f
        };
        itemRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<DataRepository.Models.Item> { existing });
        itemRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);
        productRepo.Setup(r => r.FindByIdAsync(10))
            .ReturnsAsync(new DataRepository.Models.Product
            {
                Id = 10, Sku = "EA-001", Name = "Glass", UnitPrice = 5f, AvailableUnits = 100
            });

        var svc = BuildSut(itemRepo, productRepo);
        var result = await svc.CreateItem(1, 10, 2);

        result.Should().BeTrue();
        existing.Quantity.Should().Be(5);
        itemRepo.Verify(r => r.Update(existing), Times.Once);
    }

    [Fact]
    public async Task CreateItem_ExistingDeletedItem_RestoresItemAndReturnsTrue()
    {
        var (itemRepo, productRepo) = Mocks();
        var existing = new DataRepository.Models.Item
        {
            Id = 1, IdProduct = 10, IdShoppingCart = 1, Quantity = 3, IsDeleted = true, TotalPrice = 0f
        };
        itemRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<DataRepository.Models.Item> { existing });
        itemRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);
        productRepo.Setup(r => r.FindByIdAsync(10))
            .ReturnsAsync(new DataRepository.Models.Product
            {
                Id = 10, Sku = "EA-001", Name = "Glass", UnitPrice = 5f, AvailableUnits = 100
            });

        var svc = BuildSut(itemRepo, productRepo);
        var result = await svc.CreateItem(1, 10, 4);

        result.Should().BeTrue();
        existing.IsDeleted.Should().BeFalse();
        existing.Quantity.Should().Be(4);
        itemRepo.Verify(r => r.Update(existing), Times.Once);
    }
}
