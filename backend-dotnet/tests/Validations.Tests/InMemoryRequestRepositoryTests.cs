using FluentAssertions;
using Validations;
using Validations.Model;
using Xunit;

namespace Validations.Tests;

public class InMemoryRequestRepositoryTests
{
    // Use unique usernames per test to avoid singleton state leakage
    private static string NewUser() => Guid.NewGuid().ToString();

    [Fact]
    public void GetRequest_UnknownUser_ReturnsNewRequestWithCorrectUserName()
    {
        var user = NewUser();
        var request = InMemoryRequestRepository.Instance.GetRequest(user);

        request.Should().NotBeNull();
        request.UserName.Should().Be(user);
        request.ValidationMaps.Should().BeEmpty();
    }

    [Fact]
    public void GetRequest_KnownUser_ReturnsSameStoredRequest()
    {
        var user = NewUser();
        var first = InMemoryRequestRepository.Instance.GetRequest(user);
        first.RecoveryNextHandlerName = "step-1";
        InMemoryRequestRepository.Instance.SaveRequest(first);

        var second = InMemoryRequestRepository.Instance.GetRequest(user);

        second.RecoveryNextHandlerName.Should().Be("step-1");
    }

    [Fact]
    public void SaveRequest_OverwritesExistingEntry()
    {
        var user = NewUser();
        var request = new Request
        {
            UserName = user,
            ProcessCreationDate = DateTime.Now,
            ValidationMaps = new List<ValidationMap>()
        };

        request.RecoveryNextHandlerName = "v1";
        InMemoryRequestRepository.Instance.SaveRequest(request);

        request.RecoveryNextHandlerName = "v2";
        InMemoryRequestRepository.Instance.SaveRequest(request);

        var stored = InMemoryRequestRepository.Instance.GetRequest(user);
        stored.RecoveryNextHandlerName.Should().Be("v2");
    }

    [Fact]
    public void DeleteRequest_ExistingUser_RemovesEntry()
    {
        var user = NewUser();
        var request = new Request
        {
            UserName = user,
            ProcessCreationDate = DateTime.Now,
            ValidationMaps = new List<ValidationMap>()
        };
        InMemoryRequestRepository.Instance.SaveRequest(request);

        var result = InMemoryRequestRepository.Instance.DeleteRequest(user);

        // DeleteRequest always returns null per implementation
        result.Should().BeNull();
        // After deletion, GetRequest creates a fresh one with empty ValidationMaps
        var afterDelete = InMemoryRequestRepository.Instance.GetRequest(user);
        afterDelete.ValidationMaps.Should().BeEmpty();
    }

    [Fact]
    public void DeleteRequest_UnknownUser_ReturnsNull()
    {
        var result = InMemoryRequestRepository.Instance.DeleteRequest(NewUser());
        result.Should().BeNull();
    }

    [Fact]
    public void GetFirstRequest_AfterSaving_ReturnsARequest()
    {
        var user = NewUser();
        var request = new Request
        {
            UserName = user,
            ProcessCreationDate = DateTime.Now,
            ValidationMaps = new List<ValidationMap>()
        };
        InMemoryRequestRepository.Instance.SaveRequest(request);

        var first = InMemoryRequestRepository.Instance.GetFirstRequest();
        first.Should().NotBeNull();
    }
}
