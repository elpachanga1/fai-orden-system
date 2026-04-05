using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ShoppingCartBackEnd.Controllers;
using Validations;
using Validations.Interface;
using Validations.Model;
using Xunit;

namespace CarritoComprasBackend.Tests.Controllers;

public class ProcessControllerTests
{
    private readonly Mock<ICreatorFactory> _mockCreatorFactory;
    private readonly Mock<IHandler> _mockHandler;
    private readonly ProcessController _sut;

    public ProcessControllerTests()
    {
        _mockCreatorFactory = new Mock<ICreatorFactory>();
        _mockHandler = new Mock<IHandler>();
        _mockCreatorFactory.Setup(f => f.CreateChain()).Returns(_mockHandler.Object);

        _sut = new ProcessController(_mockCreatorFactory.Object);
    }

    // ── GetProcess ────────────────────────────────────────────────────────────

    [Fact]
    public void GetProcess_ConUsername_RetornaOkConRequest()
    {
        // InMemoryRequestRepository crea automáticamente el Request si no existe
        var result = _sut.GetProcess("usuario-gp-1");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<Request>();
    }

    [Fact]
    public void GetProcess_RequestContieneElUsername()
    {
        const string username = "usuario-gp-2";

        var result = _sut.GetProcess(username);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var request = okResult.Value.Should().BeOfType<Request>().Subject;
        request.UserName.Should().Be(username);
    }

    // ── RunValidation ─────────────────────────────────────────────────────────

    [Fact]
    public void RunValidation_SinValidacionPendiente_RetornaOk()
    {
        // Request sin ValidationMaps; RecoveryNextHandlerName es null → no entra al if
        var result = _sut.RunValidation("usuario-rv-1");

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public void RunValidation_EjecutaHandlerChain()
    {
        _sut.RunValidation("usuario-rv-2");

        _mockCreatorFactory.Verify(f => f.CreateChain(), Times.Once);
        _mockHandler.Verify(h => h.Handle(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public void RunValidation_ConValidacionPendiente_MarcaStateTrue()
    {
        const string username = "usuario-rv-3";

        // Pre-poblar el repositorio con un request que tenga una validación pendiente
        var request = InMemoryRequestRepository.Instance.GetRequest(username);
        const string handlerName = "StepA";
        request.RecoveryNextHandlerName = handlerName;
        request.ValidationMaps.Add(new ValidationMap
        {
            ValidationName = handlerName,
            State = false,
            CreationDate = DateTime.MinValue
        });

        _sut.RunValidation(username);

        var validationEntry = request.ValidationMaps.First(v => v.ValidationName == handlerName);
        validationEntry.State.Should().BeTrue();
    }

    // ── DeleteProcess ─────────────────────────────────────────────────────────

    [Fact]
    public void DeleteProcess_RetornaOk()
    {
        // Aseguramos que el usuario exista previamente
        InMemoryRequestRepository.Instance.GetRequest("usuario-dp-1");

        var result = _sut.DeleteProcess("usuario-dp-1");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        // DeleteRequest siempre devuelve null
        okResult.Value.Should().BeNull();
    }
}
