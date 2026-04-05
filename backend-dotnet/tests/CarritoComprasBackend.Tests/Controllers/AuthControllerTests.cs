using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Domain.Models;
using Services.Domain.Services;
using ShoppingCartBackEnd.Controllers;
using Validations;
using Validations.Interface;
using Validations.Model;
using Xunit;

namespace CarritoComprasBackend.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<StoreService> _mockStoreService;
    private readonly Mock<ICreatorFactory> _mockCreatorFactory;
    private readonly Mock<IHandler> _mockHandler;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        // StoreService no tiene interfaz: lo mockeamos como clase concreta (métodos son virtual)
        _mockStoreService = new Mock<StoreService>(
            (ProductService?)null!, (UserService?)null!, (ItemService?)null!);
        _mockCreatorFactory = new Mock<ICreatorFactory>();
        _mockHandler = new Mock<IHandler>();

        _mockCreatorFactory.Setup(f => f.CreateChain()).Returns(_mockHandler.Object);

        _sut = new AuthController(_mockStoreService.Object, _mockCreatorFactory.Object);
    }

    // ── Credenciales válidas → 200 con el usuario ─────────────────────────────

    [Fact]
    public async Task AuthenticateUser_CredencialesValidas_RetornaOkConUsuario()
    {
        var usuario = new User { Id = 1, UserName = "juan", Name = "Juan Pérez", Password = "hash" };
        _mockStoreService.Setup(s => s.AuthenticateUser("juan", "clave123"))
                         .ReturnsAsync(usuario);

        var result = await _sut.AuthenticateUser("juan", "clave123");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(usuario);
    }

    // ── Usuario no encontrado → 404 ───────────────────────────────────────────

    [Fact]
    public async Task AuthenticateUser_UsuarioNoEncontrado_Retorna404()
    {
        _mockStoreService.Setup(s => s.AuthenticateUser(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync((User?)null);

        var result = await _sut.AuthenticateUser("noexiste", "clave");

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(404);
    }

    // ── Excepción en el servicio → 500 ────────────────────────────────────────

    [Fact]
    public async Task AuthenticateUser_ExcepcionEnServicio_Retorna500()
    {
        _mockStoreService.Setup(s => s.AuthenticateUser(It.IsAny<string>(), It.IsAny<string>()))
                         .ThrowsAsync(new Exception("Error de base de datos"));

        var result = await _sut.AuthenticateUser("juan", "clave123");

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    // ── Autenticación exitosa ejecuta el handler de validación ────────────────

    [Fact]
    public async Task AuthenticateUser_CredencialesValidas_EjecutaHandlerChain()
    {
        var usuario = new User { Id = 2, UserName = "maria", Name = "María", Password = "hash" };
        _mockStoreService.Setup(s => s.AuthenticateUser("maria", "pass"))
                         .ReturnsAsync(usuario);

        await _sut.AuthenticateUser("maria", "pass");

        _mockCreatorFactory.Verify(f => f.CreateChain(), Times.Once);
        _mockHandler.Verify(h => h.Handle(It.IsAny<Request>()), Times.Once);
    }
}
