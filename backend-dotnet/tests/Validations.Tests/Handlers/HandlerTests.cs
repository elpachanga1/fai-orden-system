using FluentAssertions;
using Moq;
using Validations;
using Validations.ConcretImplementation;
using Validations.Interface;
using Validations.Model;
using Xunit;

namespace Validations.Tests.Handlers;

/// <summary>
/// Helpers shared by all handler tests.
/// </summary>
internal static class HandlerTestHelpers
{
    internal static string NewUser() => Guid.NewGuid().ToString();

    internal static Request IncomingRequest(string user, string handlerName, bool state) =>
        new()
        {
            UserName = user,
            ProcessCreationDate = DateTime.Now,
            ValidationMaps = new List<ValidationMap>
            {
                new ValidationMap
                {
                    ValidationName = handlerName,
                    State = state,
                    CreationDate = DateTime.Now
                }
            }
        };
}

// ═══════════════════════════════════════════════════════════════
// AuthenticationHandler
// ═══════════════════════════════════════════════════════════════
public class AuthenticationHandlerTests
{
    private const string HandlerName = "Authentication";

    [Fact]
    public void Handle_ValidState_StoresValidationMapInRepository()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new AuthenticationHandler();
        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: true);

        handler.Handle(incoming);

        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        stored.ValidationMaps.Should().Contain(vm => vm.ValidationName == HandlerName && vm.State == true);
    }

    [Fact]
    public void Handle_ValidState_CallsNextHandler()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new AuthenticationHandler();
        var mockNext = new Mock<IHandler>();
        mockNext.Setup(h => h.HandlerName).Returns("MockNext");
        handler.SetNext(mockNext.Object);

        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: true);
        handler.Handle(incoming);

        mockNext.Verify(h => h.Handle(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public void Handle_FalseState_DoesNotCallNextHandler()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new AuthenticationHandler();
        var mockNext = new Mock<IHandler>();
        mockNext.Setup(h => h.HandlerName).Returns("MockNext");
        handler.SetNext(mockNext.Object);

        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: false);
        handler.Handle(incoming);

        mockNext.Verify(h => h.Handle(It.IsAny<Request>()), Times.Never);
    }

    [Fact]
    public void Handle_NoNextHandler_SetsFinishedMessage()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new AuthenticationHandler();
        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: true);

        handler.Handle(incoming);

        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        stored.RecoveryNextHandlerName.Should().Be("The Process is Finished");
    }
}

// ═══════════════════════════════════════════════════════════════
// DataSanitizationHandler
// ═══════════════════════════════════════════════════════════════
public class DataSanitizationHandlerTests
{
    private const string HandlerName = "DataSanitization";

    [Fact]
    public void Handle_ValidState_StoresValidationMapInRepository()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new DataSanitizationHandler();
        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: true);

        handler.Handle(incoming);

        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        stored.ValidationMaps.Should().Contain(vm => vm.ValidationName == HandlerName && vm.State == true);
    }

    [Fact]
    public void Handle_ValidState_CallsNextHandler()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new DataSanitizationHandler();
        var mockNext = new Mock<IHandler>();
        mockNext.Setup(h => h.HandlerName).Returns("MockNext");
        handler.SetNext(mockNext.Object);

        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: true);
        handler.Handle(incoming);

        mockNext.Verify(h => h.Handle(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public void Handle_FalseState_DoesNotCallNextHandler()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new DataSanitizationHandler();
        var mockNext = new Mock<IHandler>();
        mockNext.Setup(h => h.HandlerName).Returns("MockNext");
        handler.SetNext(mockNext.Object);

        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: false);
        handler.Handle(incoming);

        mockNext.Verify(h => h.Handle(It.IsAny<Request>()), Times.Never);
    }

    [Fact]
    public void Handle_NoNextHandler_SetsFinishedMessage()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new DataSanitizationHandler();
        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: true);

        handler.Handle(incoming);

        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        stored.RecoveryNextHandlerName.Should().Be("The Process is Finished");
    }
}

// ═══════════════════════════════════════════════════════════════
// BruteForceHandler
// ═══════════════════════════════════════════════════════════════
public class BruteForceHandlerTests
{
    private const string HandlerName = "BruteForce";

    [Fact]
    public void Handle_ValidState_StoresValidationMapInRepository()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new BruteForceHandler();
        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: true);

        handler.Handle(incoming);

        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        stored.ValidationMaps.Should().Contain(vm => vm.ValidationName == HandlerName && vm.State == true);
    }

    [Fact]
    public void Handle_ValidState_CallsNextHandler()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new BruteForceHandler();
        var mockNext = new Mock<IHandler>();
        mockNext.Setup(h => h.HandlerName).Returns("MockNext");
        handler.SetNext(mockNext.Object);

        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: true);
        handler.Handle(incoming);

        mockNext.Verify(h => h.Handle(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public void Handle_FalseState_DoesNotCallNextHandler()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new BruteForceHandler();
        var mockNext = new Mock<IHandler>();
        mockNext.Setup(h => h.HandlerName).Returns("MockNext");
        handler.SetNext(mockNext.Object);

        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: false);
        handler.Handle(incoming);

        mockNext.Verify(h => h.Handle(It.IsAny<Request>()), Times.Never);
    }

    [Fact]
    public void Handle_NoNextHandler_SetsFinishedMessage()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new BruteForceHandler();
        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: true);

        handler.Handle(incoming);

        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        stored.RecoveryNextHandlerName.Should().Be("The Process is Finished");
    }
}

// ═══════════════════════════════════════════════════════════════
// ResponseSpeedHandler
// ═══════════════════════════════════════════════════════════════
public class ResponseSpeedHandlerTests
{
    private const string HandlerName = "ResponseSpeed";

    [Fact]
    public void Handle_ValidState_StoresValidationMapInRepository()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new ResponseSpeedHandler();
        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: true);

        handler.Handle(incoming);

        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        stored.ValidationMaps.Should().Contain(vm => vm.ValidationName == HandlerName && vm.State == true);
    }

    [Fact]
    public void Handle_ValidState_CallsNextHandler()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new ResponseSpeedHandler();
        var mockNext = new Mock<IHandler>();
        mockNext.Setup(h => h.HandlerName).Returns("MockNext");
        handler.SetNext(mockNext.Object);

        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: true);
        handler.Handle(incoming);

        mockNext.Verify(h => h.Handle(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public void Handle_FalseState_DoesNotCallNextHandler()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new ResponseSpeedHandler();
        var mockNext = new Mock<IHandler>();
        mockNext.Setup(h => h.HandlerName).Returns("MockNext");
        handler.SetNext(mockNext.Object);

        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: false);
        handler.Handle(incoming);

        mockNext.Verify(h => h.Handle(It.IsAny<Request>()), Times.Never);
    }

    [Fact]
    public void Handle_NoNextHandler_SetsFinishedMessage()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new ResponseSpeedHandler();
        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: true);

        handler.Handle(incoming);

        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        stored.RecoveryNextHandlerName.Should().Be("The Process is Finished");
    }
}

// ═══════════════════════════════════════════════════════════════
// FinishValidationHandler
// ═══════════════════════════════════════════════════════════════
public class FinishValidationHandlerTests
{
    private const string HandlerName = "FinishValidation";

    [Fact]
    public void Handle_AlwayySetsProcessFinished()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new FinishValidationHandler();
        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: true);

        handler.Handle(incoming);

        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        stored.RecoveryNextHandlerName.Should().Be("The Process is Finished");
    }

    [Fact]
    public void Handle_ValidState_StoresValidationMap()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new FinishValidationHandler();
        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: true);

        handler.Handle(incoming);

        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        stored.ValidationMaps.Should().Contain(vm => vm.ValidationName == HandlerName && vm.State == true);
    }

    [Fact]
    public void Handle_FalseState_StillSetsProcessFinished()
    {
        var user = HandlerTestHelpers.NewUser();
        var handler = new FinishValidationHandler();
        var incoming = HandlerTestHelpers.IncomingRequest(user, HandlerName, state: false);

        handler.Handle(incoming);

        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        stored.RecoveryNextHandlerName.Should().Be("The Process is Finished");
    }
}
