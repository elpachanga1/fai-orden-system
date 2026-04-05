using DataRepository.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Services.Domain.Services;
using ShoppingCartBackEnd.Factories;
using Validations;
using Xunit;

namespace CarritoComprasBackend.Tests.Factories;

/// <summary>
/// Pruebas de integración de DI para cada método de ServiceCollectionExtensions.
/// Verifican que los servicios se registran correctamente y pueden resolverse.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un mock de IConfiguration con las claves requeridas por AuthenticationHelper y SessionService.
    /// </summary>
    private static IConfiguration BuildConfigMock()
    {
        var mock = new Mock<IConfiguration>();
        mock.Setup(c => c["auth:secretKey"]).Returns("clave-secreta-de-prueba-al-menos-32-caracteres!");
        mock.Setup(c => c["auth:authActivityTime"]).Returns("30");
        return mock.Object;
    }

    /// <summary>
    /// Registra todos los IRepository<T> necesarios como mocks en el contenedor.
    /// </summary>
    private static void RegisterRepositoryMocks(IServiceCollection services)
    {
        services.AddSingleton(Mock.Of<IRepository<DataRepository.Models.Product>>());
        services.AddSingleton(Mock.Of<IRepository<DataRepository.Models.Item>>());
        services.AddSingleton(Mock.Of<IRepository<DataRepository.Models.ShoppingCart>>());
        services.AddSingleton(Mock.Of<IRepository<DataRepository.Models.User>>());
        services.AddSingleton(Mock.Of<IRepository<DataRepository.Models.Session>>());
    }

    // ── AddProductServices ────────────────────────────────────────────────────

    [Fact]
    public void AddProductServices_RegistraProductService_Resolvible()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<AutoMapper.IMapper>());
        services.AddSingleton(Mock.Of<IRepository<DataRepository.Models.Product>>());

        services.AddProductServices();

        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<ProductService>();
        svc.Should().NotBeNull();
    }

    // ── AddSessionServices ────────────────────────────────────────────────────

    [Fact]
    public void AddSessionServices_RegistraSessionService_Resolvible()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<AutoMapper.IMapper>());
        services.AddSingleton(BuildConfigMock());
        services.AddSingleton(Mock.Of<IRepository<DataRepository.Models.Session>>());

        services.AddSessionServices();

        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<SessionService>();
        svc.Should().NotBeNull();
    }

    // ── AddItemServices ───────────────────────────────────────────────────────

    [Fact]
    public void AddItemServices_RegistraItemService_Resolvible()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<AutoMapper.IMapper>());
        services.AddSingleton(Mock.Of<IRepository<DataRepository.Models.Product>>());
        services.AddSingleton(Mock.Of<IRepository<DataRepository.Models.Item>>());

        services.AddProductServices();
        services.AddItemServices();

        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<ItemService>();
        svc.Should().NotBeNull();
    }

    // ── AddShoppingCartServices ───────────────────────────────────────────────

    [Fact]
    public void AddShoppingCartServices_RegistraShoppingCartService_Resolvible()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<AutoMapper.IMapper>());
        services.AddSingleton(Mock.Of<IRepository<DataRepository.Models.Product>>());
        services.AddSingleton(Mock.Of<IRepository<DataRepository.Models.Item>>());
        services.AddSingleton(Mock.Of<IRepository<DataRepository.Models.ShoppingCart>>());

        services.AddProductServices();
        services.AddItemServices();
        services.AddShoppingCartServices();

        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<ShoppingCartService>();
        svc.Should().NotBeNull();
    }

    // ── AddUserServices ───────────────────────────────────────────────────────

    [Fact]
    public void AddUserServices_RegistraUserService_Resolvible()
    {
        var services = new ServiceCollection();
        var config = BuildConfigMock();
        services.AddSingleton(Mock.Of<AutoMapper.IMapper>());
        services.AddSingleton(config);
        RegisterRepositoryMocks(services);

        services.AddProductServices();
        services.AddItemServices();
        services.AddSessionServices();
        services.AddShoppingCartServices();
        services.AddUserServices();

        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<UserService>();
        svc.Should().NotBeNull();
    }

    // ── AddStoreServices ──────────────────────────────────────────────────────

    [Fact]
    public void AddStoreServices_RegistraStoreService_Resolvible()
    {
        var services = new ServiceCollection();
        var config = BuildConfigMock();
        services.AddSingleton(Mock.Of<AutoMapper.IMapper>());
        services.AddSingleton(config);
        RegisterRepositoryMocks(services);

        services.AddProductServices();
        services.AddItemServices();
        services.AddSessionServices();
        services.AddShoppingCartServices();
        services.AddUserServices();
        services.AddStoreServices();

        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<StoreService>();
        svc.Should().NotBeNull();
    }

    // ── AddValidationChainService ─────────────────────────────────────────────

    [Fact]
    public void AddValidationChainService_RegistraICreatorFactory_Resolvible()
    {
        var services = new ServiceCollection();
        services.AddValidationChainService();

        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<ICreatorFactory>();
        svc.Should().NotBeNull();
    }
}
