using AutoMapper;
using DataRepository.Repositories;
using Microsoft.Extensions.Configuration;
using Services.Domain.Helpers;

namespace Services.Domain.Services
{
    public class UserService
    {
        private readonly IMapper _mapper;
        private readonly ShoppingCartService _shoppingCartService;
        private readonly SessionService _sessionService;
        private IRepository<DataRepository.Models.User> _userRepository;
        private readonly IConfiguration _configuration;

        private AuthenticationHelper _authenticationHelper;

        public UserService(
            IMapper mapper,
            IConfiguration configuration,
            ShoppingCartService shoppingCartService,
            SessionService sessionService,
            IRepository<DataRepository.Models.User> userRepository) 
        {
            _mapper = mapper;
            _configuration = configuration;
            _shoppingCartService = shoppingCartService;
            _sessionService = sessionService;
            _userRepository = userRepository;

            _authenticationHelper = new AuthenticationHelper(_configuration);
        }

        public async Task<Models.User?> AuthenticateUser(string username, string password)
        {
            Models.User? userDomainEntity = null;
            string encryptedPassword = _authenticationHelper.Hash(password);

            var userDataEntity = (await _userRepository.GetAllAsync())
                ?.FirstOrDefault(user => user.UserName == username && user.Password == encryptedPassword);
            
            if (userDataEntity != null)
            {
                userDomainEntity = _mapper.Map<Models.User>(userDataEntity);
                userDomainEntity.Password = string.Empty;
                Models.Session? session = await _sessionService.AddSession(userDomainEntity);
                session.Token = _authenticationHelper.GenerateJWTToken(userDataEntity);
                userDomainEntity.SessionReference = session;
            }
            return userDomainEntity;
        }

        #region Business logic for Shopping Cart (mover a otro archivo)
        public async Task<bool> AddProductToShoppingCart(string IdUser, int IdProduct, int Quantity)
        {
            bool result = await _shoppingCartService.AddProductToShoppingCart(IdUser, IdProduct, Quantity);
            return result;
        }

        public async Task<bool> DeleteProductFromShoppingCart(string IdUser, int IdItem)
        {
            bool result = false;
            result = await _shoppingCartService.DeleteProductFromShoppingCart(IdUser, IdItem);
            return result;
        }

        public async Task<bool> EmptyShoppingCart(string IdUser)
        {
            bool result = false;
            result = await _shoppingCartService.EmptyShoppingCart(IdUser);
            return result;
        }

        public async Task<bool> CompleteShoppingCart(string IdUser)
        {
            bool result = false;
            result = await _shoppingCartService.CompleteShoppingCart(IdUser);
            return result;
        }
        #endregion


        public async Task<IEnumerable<Models.Item>> GetItemsByProductId(int ProductId)
        {
            return await _shoppingCartService.GetItemsByProductId(ProductId);
        }

        public async Task<IEnumerable<Models.Item>> GetAllItems()
        {
            return await _shoppingCartService.GetAllItems();
        }

        public async Task<float> GetTotalSales()
        {
            return await _shoppingCartService.GetTotalSales();
        }
    }
}
