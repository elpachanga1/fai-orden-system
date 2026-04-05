using AutoMapper;
using DataRepository.Repositories;

namespace Services.Domain.Services
{
    public class ShoppingCartService
    {
        private readonly IMapper _mapper;
        private IRepository<DataRepository.Models.ShoppingCart> _shoppingCartRepository;
        private readonly ItemService _itemService;

        public ShoppingCartService(IMapper mapper, IRepository<DataRepository.Models.ShoppingCart> shoppingCartRepository, ItemService itemService)
        {
            _mapper = mapper;
            this._shoppingCartRepository = shoppingCartRepository;
            this._itemService = itemService;
        }

        public async Task<bool> AddProductToShoppingCart(string IdUser, int IdProduct, int Quantity)
        {
            bool result = false;

            Models.ShoppingCart? shoppingCart = await GetShoppingCart(IdUser, false);
            if (shoppingCart == null)
            {
                int IdShoppingCart = await CreateShoppingCart(IdUser);
                shoppingCart = await GetShoppingCartById(IdShoppingCart);
                result = true;
            }
            await _itemService.CreateItem(shoppingCart.Id, IdProduct, Quantity);

            return result;
        }

        public async Task<bool> CompleteShoppingCart(string IdUser)
        {
            bool result = false;

            var shoppingCartsDataEntity = await _shoppingCartRepository.GetAllAsync();

            var shoppingCart = shoppingCartsDataEntity.FirstOrDefault(
                dataShoppingCart => dataShoppingCart.IdUser == IdUser && dataShoppingCart.IsCompleted == false
            );

            if (shoppingCart != null)
            {
                shoppingCart.FinishDate = DateTime.UtcNow;
                shoppingCart.UpdatedDate = DateTime.UtcNow;
                shoppingCart.IsCompleted = true;
                var entity = _mapper.Map<DataRepository.Models.ShoppingCart>(shoppingCart);
                _shoppingCartRepository.Update(entity);
                await _shoppingCartRepository.SaveAsync();                
            }

            return result;
        }


        public async Task<bool> DeleteProductFromShoppingCart(string IdUser, int IdItem)
        {
            bool result = false;

            Models.ShoppingCart? shoppingCart = await GetShoppingCart(IdUser, false);
            if (shoppingCart != null)
            {
                result = await _itemService.DeleteItem(IdItem);            }
            
            return result;
        }


        public async Task<bool> EmptyShoppingCart(string IdUser)
        {
            bool result = false;

            Models.ShoppingCart? shoppingCart = await GetShoppingCart(IdUser, false);
            if (shoppingCart != null)
            {
                result = await _itemService.DeleteItems();
            }

            return result;
        }


        private async Task<Models.ShoppingCart?> GetShoppingCart(string IdUser, bool IsCompleted)
        {
            Models.ShoppingCart? shoppingCartDomainEntity = null;

            var shoppingCartsDataEntity = await _shoppingCartRepository.GetAllAsync();

            foreach (var dataShoppingCart in shoppingCartsDataEntity)
            {
                if (dataShoppingCart.IdUser == IdUser && dataShoppingCart.IsCompleted == IsCompleted)
                {
                    shoppingCartDomainEntity = _mapper.Map<Models.ShoppingCart>(dataShoppingCart);
                    break;
                }
            }
            return shoppingCartDomainEntity;
        }

        private async Task<Models.ShoppingCart> GetShoppingCartById(int Id)
        {
            Models.ShoppingCart? shoppingCartDomainEntity = null;

            var shoppingCartsDataEntity = await _shoppingCartRepository.FindByIdAsync(Id);
            
            shoppingCartDomainEntity = _mapper.Map<Models.ShoppingCart>(shoppingCartsDataEntity);
            return shoppingCartDomainEntity;
        }

        private async Task<int> CreateShoppingCart(string IdUser)
        {
            Models.ShoppingCart shoppingCartDomainEntity = new Models.ShoppingCart();

            shoppingCartDomainEntity.IdUser = IdUser;
            shoppingCartDomainEntity.CreationDate = DateTime.UtcNow;
            shoppingCartDomainEntity.UpdatedDate = shoppingCartDomainEntity.CreationDate;
            shoppingCartDomainEntity.IsCompleted = false;
            
            DataRepository.Models.ShoppingCart shoppingCartDataEntity = _mapper.Map<DataRepository.Models.ShoppingCart>(shoppingCartDomainEntity);
            await _shoppingCartRepository.AddAsync(shoppingCartDataEntity);
            await _shoppingCartRepository.SaveAsync();            
            
            return shoppingCartDataEntity.Id;        
        }

        public async Task<IEnumerable<Models.Item>> GetItemsByProductId(int ProductId)
        {
            List<Models.Item> filteredItems = new List<Models.Item>();
            var shoppingCartsDataEntity = await _shoppingCartRepository.GetAllAsync();
            var completedShoppingCart = shoppingCartsDataEntity.OrderBy(dataShoppingCart => dataShoppingCart.Id)
                .FirstOrDefault(dataShoppingCart => dataShoppingCart.IsCompleted == false);
            
            if (completedShoppingCart != null)
            {
                var shoppingCartDomainEntity = _mapper.Map<Models.ShoppingCart>(completedShoppingCart);
                filteredItems = await _itemService.GetItemsByShoppingCartByProductId(shoppingCartDomainEntity.Id, ProductId);
            }            

            return filteredItems;
        }

        public async Task<IEnumerable<Models.Item>> GetAllItems()
        {
            List<Models.Item> filteredItems = new List<Models.Item>();
            
            var shoppingCartsDataEntity = await _shoppingCartRepository.GetAllAsync();
            var completedShoppingCart = shoppingCartsDataEntity.OrderBy(dataShoppingCart => dataShoppingCart.Id)
                .FirstOrDefault(dataShoppingCart => dataShoppingCart.IsCompleted == false);

            if (completedShoppingCart != null)
            {
                var shoppingCartDomainEntity = _mapper.Map<Models.ShoppingCart>(completedShoppingCart);
                var allItems = await _itemService.GetAllItems();
                foreach (var item in allItems)
                {
                    if(item.IdShoppingCart == shoppingCartDomainEntity.Id)
                    {
                        filteredItems.Add(item);
                    }
                }
            }

            return filteredItems;
        }

        public async Task<float> GetTotalSales()
        {
            float totalSales = 0;
            var shoppingCartsDataEntity = await _shoppingCartRepository.GetAllAsync();
            var completedShoppingCarts = shoppingCartsDataEntity
                .Where(dataShoppingCart => dataShoppingCart.IsCompleted == true)
                .OrderBy(dataShoppingCart => dataShoppingCart.Id)
                .ToList();

            if (completedShoppingCarts.Count > 0)
            {
                var ItemsEntity = await _itemService.GetAllItems();
                foreach ( var car in completedShoppingCarts)
                {
                    var shoppingCartDomainEntity = _mapper.Map<Models.ShoppingCart>(car);
                    shoppingCartDomainEntity.items = ItemsEntity
                        .Where(item => item.IdShoppingCart == shoppingCartDomainEntity.Id).ToList();
                    totalSales = totalSales + shoppingCartDomainEntity.CalculateTotal();
                }
            }
            return totalSales;
        }
    }
}
