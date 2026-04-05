using AutoMapper;
using DataRepository.Repositories;
using Microsoft.Extensions.Configuration;

namespace Services.Domain.Services
{
    public class SessionService
    {
        private readonly IMapper _mapper;
        private IRepository<DataRepository.Models.Session> _sessionRepository;
        private readonly IConfiguration _config;
        private readonly int authActivityTime;

        public SessionService(
            IMapper mapper,
            IConfiguration configuration,
            IRepository<DataRepository.Models.Session> sessionRepository)
        {
            _mapper = mapper;
            _config = configuration;
            _sessionRepository = sessionRepository;
            authActivityTime = int.Parse(_config["auth:authActivityTime"]);
        }

        public async Task<Models.Session> AddSession(Models.User user)
        {
            Models.Session sessionDomainEntity = new Models.Session
            {
                UserId = user.Id,
                SessionStart = DateTime.Now,
                SessionEnd = DateTime.Now.AddMinutes(authActivityTime),
            };

            DataRepository.Models.Session sessionDataEntity = _mapper.Map<DataRepository.Models.Session>(sessionDomainEntity);
            await _sessionRepository.AddAsync(sessionDataEntity);
            await _sessionRepository.SaveAsync();

            return sessionDomainEntity;
        }
    }
}
