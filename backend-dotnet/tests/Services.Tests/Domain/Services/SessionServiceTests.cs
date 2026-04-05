using DataRepository.Repositories;
using FluentAssertions;
using Moq;
using Services.Domain.Models;
using Services.Domain.Services;
using Xunit;

namespace Services.Tests.Domain.Services;

public class SessionServiceTests
{
    private readonly IMapper _mapper = TestHelpers.CreateMapper();
    private readonly Microsoft.Extensions.Configuration.IConfiguration _config = TestHelpers.CreateConfiguration();

    // ---------------------------------------------------------------
    // AddSession
    // ---------------------------------------------------------------

    [Fact]
    public async Task AddSession_ValidUser_ReturnsSessionWithCorrectUserId()
    {
        var sessionRepo = new Mock<IRepository<DataRepository.Models.Session>>();
        sessionRepo.Setup(r => r.AddAsync(It.IsAny<DataRepository.Models.Session>())).Returns(Task.CompletedTask);
        sessionRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var svc = new SessionService(_mapper, _config, sessionRepo.Object);
        var user = new User { Id = 42, UserName = "john", Name = "John Doe", Password = "hashed" };

        var result = await svc.AddSession(user);

        result.Should().NotBeNull();
        result.UserId.Should().Be(42);
        result.SessionStart.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        result.SessionEnd.Should().BeAfter(result.SessionStart);
    }

    [Fact]
    public async Task AddSession_ValidUser_PersistsSessionToRepository()
    {
        var sessionRepo = new Mock<IRepository<DataRepository.Models.Session>>();
        sessionRepo.Setup(r => r.AddAsync(It.IsAny<DataRepository.Models.Session>())).Returns(Task.CompletedTask);
        sessionRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var svc = new SessionService(_mapper, _config, sessionRepo.Object);
        var user = new User { Id = 7, UserName = "jane", Name = "Jane Doe", Password = "hashed" };

        await svc.AddSession(user);

        sessionRepo.Verify(r => r.AddAsync(It.Is<DataRepository.Models.Session>(s => s.UserId == 7)), Times.Once);
        sessionRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public void Constructor_MissingAuthActivityTime_ThrowsInvalidOperationException()
    {
        var badConfig = TestHelpers.CreateConfiguration(new Dictionary<string, string?>
        {
            ["auth:authActivityTime"] = null
        });
        var sessionRepo = new Mock<IRepository<DataRepository.Models.Session>>();

        var act = () => new SessionService(_mapper, badConfig, sessionRepo.Object);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*authActivityTime*");
    }
}
