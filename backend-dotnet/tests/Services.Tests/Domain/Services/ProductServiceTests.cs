using DataRepository.Repositories;
using FluentAssertions;
using Moq;
using Services.Domain.Services;
using Xunit;

namespace Services.Tests.Domain.Services;

public class ProductServiceTests
{
    private readonly IMapper _mapper = TestHelpers.CreateMapper();

    private Mock<IRepository<DataRepository.Models.Product>> RepoMock() =>
        new Mock<IRepository<DataRepository.Models.Product>>();

    // ---------------------------------------------------------------
    // GetProductById
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetProductById_ExistingId_ReturnsMappedProduct()
    {
        var repo = RepoMock();
        repo.Setup(r => r.FindByIdAsync(1))
            .ReturnsAsync(new DataRepository.Models.Product
            {
                Id = 1, Sku = "SKU-001", Name = "Laptop",
                UnitPrice = 999.99f, AvailableUnits = 5
            });

        var svc = new ProductService(_mapper, repo.Object);
        var result = await svc.GetProductById(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Sku.Should().Be("SKU-001");
        result.Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task GetProductById_NonExistingId_ReturnsNull()
    {
        var repo = RepoMock();
        repo.Setup(r => r.FindByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((DataRepository.Models.Product?)null);

        var svc = new ProductService(_mapper, repo.Object);
        var result = await svc.GetProductById(999);

        result.Should().BeNull();
    }

    // ---------------------------------------------------------------
    // GetAllProducts
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetAllProducts_WithProducts_ReturnsAllMapped()
    {
        var repo = RepoMock();
        repo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<DataRepository.Models.Product>
            {
                new() { Id = 1, Sku = "A", Name = "Prod A", UnitPrice = 10f, AvailableUnits = 1 },
                new() { Id = 2, Sku = "B", Name = "Prod B", UnitPrice = 20f, AvailableUnits = 2 }
            });

        var svc = new ProductService(_mapper, repo.Object);
        var result = await svc.GetAllProducts();

        result.Should().HaveCount(2);
        result.Select(p => p.Sku).Should().BeEquivalentTo("A", "B");
    }

    [Fact]
    public async Task GetAllProducts_EmptyRepo_ReturnsEmptyList()
    {
        var repo = RepoMock();
        repo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<DataRepository.Models.Product>());

        var svc = new ProductService(_mapper, repo.Object);
        var result = await svc.GetAllProducts();

        result.Should().BeEmpty();
    }

    // ---------------------------------------------------------------
    // AddProduct
    // ---------------------------------------------------------------

    [Fact]
    public async Task AddProduct_ValidData_CallsAddAndSaveAndReturnsTrue()
    {
        var repo = RepoMock();
        repo.Setup(r => r.AddAsync(It.IsAny<DataRepository.Models.Product>()))
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var svc = new ProductService(_mapper, repo.Object);
        var result = await svc.AddProduct("SKU-NEW", "New Product", "Desc", 10, 50f, null);

        result.Should().BeTrue();
        repo.Verify(r => r.AddAsync(It.Is<DataRepository.Models.Product>(p =>
            p.Sku == "SKU-NEW" && p.Name == "New Product")), Times.Once);
        repo.Verify(r => r.SaveAsync(), Times.Once);
    }
}
