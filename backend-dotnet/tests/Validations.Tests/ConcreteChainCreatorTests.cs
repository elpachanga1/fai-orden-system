using FluentAssertions;
using Validations;
using Validations.Interface;
using Validations.Model;
using Xunit;

namespace Validations.Tests;

public class ConcreteChainCreatorTests
{
    [Fact]
    public void CreateChain_ReturnsAuthenticationHandlerAsHead()
    {
        var creator = new ConcreteChainCreator();
        var chain = creator.CreateChain();

        chain.Should().NotBeNull();
        chain.HandlerName.Should().Be("Authentication");
    }

    [Fact]
    public void CreateChain_ImplementsICreatorFactory()
    {
        ICreatorFactory creator = new ConcreteChainCreator();
        creator.Should().NotBeNull();
    }
}

public class ChainIntegrationTests
{
    private static Request BuildFullRequest(string user) => new()
    {
        UserName = user,
        ProcessCreationDate = DateTime.Now,
        ValidationMaps = new List<ValidationMap>
        {
            new() { ValidationName = "Authentication",    State = true, CreationDate = DateTime.Now },
            new() { ValidationName = "DataSanitization", State = true, CreationDate = DateTime.Now },
            new() { ValidationName = "BruteForce",       State = true, CreationDate = DateTime.Now },
            new() { ValidationName = "ResponseSpeed",    State = true, CreationDate = DateTime.Now },
            new() { ValidationName = "FinishValidation", State = true, CreationDate = DateTime.Now },
        }
    };

    [Fact]
    public void FullChain_AuthPasses_SetsRecoveryNextHandlerToDataSanitization()
    {
        var user = Guid.NewGuid().ToString();
        var chain = new ConcreteChainCreator().CreateChain();
        var request = new Request
        {
            UserName = user,
            ProcessCreationDate = DateTime.Now,
            ValidationMaps = new List<ValidationMap>
            {
                new() { ValidationName = "Authentication", State = true, CreationDate = DateTime.Now }
            }
        };

        chain.Handle(request);

        // Auth passes → handler sets RecoveryNextHandlerName to the next step before forwarding
        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        stored.RecoveryNextHandlerName.Should().Be("DataSanitization");
        stored.ValidationMaps.Should().Contain(vm => vm.ValidationName == "Authentication" && vm.State == true);
    }

    [Fact]
    public void FullChain_AuthPasses_AuthenticationValidationMapStoredWithStateTrue()
    {
        var user = Guid.NewGuid().ToString();
        var chain = new ConcreteChainCreator().CreateChain();
        var request = new Request
        {
            UserName = user,
            ProcessCreationDate = DateTime.Now,
            ValidationMaps = new List<ValidationMap>
            {
                new() { ValidationName = "Authentication", State = true, CreationDate = DateTime.Now }
            }
        };

        chain.Handle(request);

        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        stored.ValidationMaps.Should().Contain(vm =>
            vm.ValidationName == "Authentication" && vm.State == true);
    }

    [Fact]
    public void FullChain_AuthenticationFails_StopsAtAuthentication()
    {
        var user = Guid.NewGuid().ToString();
        var chain = new ConcreteChainCreator().CreateChain();

        // Only Authentication has State=false → chain stops immediately
        var request = new Request
        {
            UserName = user,
            ProcessCreationDate = DateTime.Now,
            ValidationMaps = new List<ValidationMap>
            {
                new() { ValidationName = "Authentication", State = false, CreationDate = DateTime.Now }
            }
        };
        chain.Handle(request);

        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        // DataSanitization was never reached → its ValidationMap should not be stored with State=true
        stored.ValidationMaps
            .Where(vm => vm.ValidationName == "DataSanitization")
            .Should().NotContain(vm => vm.State == true);
    }

    [Fact]
    public void FullChain_BruteForceFails_StopsBeforeResponseSpeed()
    {
        var user = Guid.NewGuid().ToString();
        var chain = new ConcreteChainCreator().CreateChain();

        var request = new Request
        {
            UserName = user,
            ProcessCreationDate = DateTime.Now,
            ValidationMaps = new List<ValidationMap>
            {
                new() { ValidationName = "Authentication",    State = true,  CreationDate = DateTime.Now },
                new() { ValidationName = "DataSanitization", State = true,  CreationDate = DateTime.Now },
                new() { ValidationName = "BruteForce",       State = false, CreationDate = DateTime.Now },
            }
        };
        chain.Handle(request);

        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        stored.ValidationMaps
            .Where(vm => vm.ValidationName == "ResponseSpeed")
            .Should().NotContain(vm => vm.State == true);
    }
}
