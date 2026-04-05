using DataRepository.Data;
using DataRepository.Models;
using DataRepository.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DataRepository.Tests.Repositories;

public class RepositoryTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    // ---------------------------------------------------------------
    // GetAllAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_EmptyTable_ReturnsEmptyCollection()
    {
        using var context = CreateContext(nameof(GetAllAsync_EmptyTable_ReturnsEmptyCollection));
        var repo = new Repository<Product>(context);

        var result = await repo.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithRecords_ReturnsAllEntities()
    {
        using var context = CreateContext(nameof(GetAllAsync_WithRecords_ReturnsAllEntities));
        context.Products.AddRange(
            new Product { Id = 1, Sku = "SKU-001", Name = "Product A", UnitPrice = 10.0f, AvailableUnits = 5 },
            new Product { Id = 2, Sku = "SKU-002", Name = "Product B", UnitPrice = 20.0f, AvailableUnits = 3 }
        );
        await context.SaveChangesAsync();

        var repo = new Repository<Product>(context);
        var result = await repo.GetAllAsync();

        result.Should().HaveCount(2);
    }

    // ---------------------------------------------------------------
    // FindByIdAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task FindByIdAsync_ExistingId_ReturnsCorrectEntity()
    {
        using var context = CreateContext(nameof(FindByIdAsync_ExistingId_ReturnsCorrectEntity));
        var product = new Product { Id = 1, Sku = "SKU-001", Name = "Product A", UnitPrice = 10.0f, AvailableUnits = 5 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repo = new Repository<Product>(context);
        var result = await repo.FindByIdAsync(1);

        result.Should().NotBeNull();
        result!.Sku.Should().Be("SKU-001");
        result.Name.Should().Be("Product A");
    }

    [Fact]
    public async Task FindByIdAsync_NonExistingId_ReturnsNull()
    {
        using var context = CreateContext(nameof(FindByIdAsync_NonExistingId_ReturnsNull));
        var repo = new Repository<Product>(context);

        var result = await repo.FindByIdAsync(999);

        result.Should().BeNull();
    }

    // ---------------------------------------------------------------
    // AddAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task AddAsync_NewEntity_EntityIsPersisted()
    {
        using var context = CreateContext(nameof(AddAsync_NewEntity_EntityIsPersisted));
        var repo = new Repository<Product>(context);
        var product = new Product { Id = 1, Sku = "SKU-NEW", Name = "New Product", UnitPrice = 15.0f, AvailableUnits = 10 };

        await repo.AddAsync(product);
        await repo.SaveAsync();

        var saved = await context.Products.FindAsync(1);
        saved.Should().NotBeNull();
        saved!.Sku.Should().Be("SKU-NEW");
    }

    [Fact]
    public async Task AddAsync_MultipleEntities_AllArePersisted()
    {
        using var context = CreateContext(nameof(AddAsync_MultipleEntities_AllArePersisted));
        var repo = new Repository<Product>(context);

        await repo.AddAsync(new Product { Id = 1, Sku = "A", Name = "A", UnitPrice = 1.0f, AvailableUnits = 1 });
        await repo.AddAsync(new Product { Id = 2, Sku = "B", Name = "B", UnitPrice = 2.0f, AvailableUnits = 2 });
        await repo.SaveAsync();

        var all = await context.Products.ToListAsync();
        all.Should().HaveCount(2);
    }

    // ---------------------------------------------------------------
    // Update
    // ---------------------------------------------------------------

    [Fact]
    public async Task Update_ExistingEntity_ChangesArePersisted()
    {
        using var context = CreateContext(nameof(Update_ExistingEntity_ChangesArePersisted));
        var product = new Product { Id = 1, Sku = "SKU-001", Name = "Original", UnitPrice = 10.0f, AvailableUnits = 5 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repo = new Repository<Product>(context);
        product.Name = "Updated";
        product.UnitPrice = 99.0f;
        repo.Update(product);
        await repo.SaveAsync();

        var updated = await context.Products.FindAsync(1);
        updated!.Name.Should().Be("Updated");
        updated.UnitPrice.Should().Be(99.0f);
    }

    // ---------------------------------------------------------------
    // Remove
    // ---------------------------------------------------------------

    [Fact]
    public async Task Remove_ExistingEntity_EntityIsDeleted()
    {
        using var context = CreateContext(nameof(Remove_ExistingEntity_EntityIsDeleted));
        var product = new Product { Id = 1, Sku = "SKU-001", Name = "To Delete", UnitPrice = 10.0f, AvailableUnits = 1 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repo = new Repository<Product>(context);
        repo.Remove(product);
        await repo.SaveAsync();

        var deleted = await context.Products.FindAsync(1);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Remove_OneOfMany_OnlyTargetIsDeleted()
    {
        using var context = CreateContext(nameof(Remove_OneOfMany_OnlyTargetIsDeleted));
        var product1 = new Product { Id = 1, Sku = "A", Name = "Keep", UnitPrice = 1.0f, AvailableUnits = 1 };
        var product2 = new Product { Id = 2, Sku = "B", Name = "Delete", UnitPrice = 2.0f, AvailableUnits = 1 };
        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        var repo = new Repository<Product>(context);
        repo.Remove(product2);
        await repo.SaveAsync();

        var remaining = await context.Products.ToListAsync();
        remaining.Should().HaveCount(1);
        remaining[0].Id.Should().Be(1);
    }

    // ---------------------------------------------------------------
    // SaveAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task SaveAsync_WithoutPendingChanges_CompletesSuccessfully()
    {
        using var context = CreateContext(nameof(SaveAsync_WithoutPendingChanges_CompletesSuccessfully));
        var repo = new Repository<Product>(context);

        var act = async () => await repo.SaveAsync();

        await act.Should().NotThrowAsync();
    }
}
