using AutoMapper;
using BusinessRules.RulesForPrice.Handlers;
using DataRepository.Repositories;

namespace Services.Domain.Services
{
    public class ItemService
    {
        private readonly IMapper _mapper;
        private IRepository<DataRepository.Models.Item> _itemRepository;
        private readonly ProductService _productService;
        
        public ItemService(IMapper mapper, IRepository<DataRepository.Models.Item> itemRepository, ProductService productService)
        {
            _mapper = mapper;
            this._itemRepository = itemRepository;
            this._productService = productService; 
        }

        public async Task<bool> DeleteItem(int IdItem)
        {
            bool result = false;

            var itemDataEntity = await _itemRepository.FindByIdAsync(IdItem);
            
            if (itemDataEntity != null) 
            { 
                /*itemDataEntity.IsDeleted = true;
                _itemRepository.Update(itemDataEntity);*/
                _itemRepository.Remove(itemDataEntity);
                await _itemRepository.SaveAsync();
                result = true;
            }
            return result;
        }

        public async Task<bool> DeleteItems()
        {
            bool result = false;

            var itemDataEntity = await _itemRepository.GetAllAsync();
            foreach (var item in itemDataEntity)
            {
                _itemRepository.Remove(item);
            }
            await _itemRepository.SaveAsync();
            return result;
        }

        public async Task<bool> CreateItem(int IdShoppingCart, int IdProduct, int Quantity)
        {
            var currentItem = (await _itemRepository.GetAllAsync()).FirstOrDefault(x => x.IdProduct == IdProduct);
            Models.Product? productDomainEntity = await _productService.GetProductById(IdProduct);

            if (currentItem == null)
            {
                Models.Item itemDomainEntity = new Models.Item();

                itemDomainEntity.IdShoppingCart = IdShoppingCart;
                itemDomainEntity.IdProduct = IdProduct;
                itemDomainEntity.Quantity = Quantity;
                itemDomainEntity.IsDeleted = false;
                itemDomainEntity.TotalPrice = PriceCalculatorHandler.GetInstance().CalculateItemPrice(
                    productDomainEntity.Sku,
                    Quantity,
                    productDomainEntity.UnitPrice);

                DataRepository.Models.Item shoppingCartDataEntity = _mapper.Map<DataRepository.Models.Item>(itemDomainEntity);
                await _itemRepository.AddAsync(shoppingCartDataEntity);
            }
            else
            {
                if (currentItem.IsDeleted)
                {
                    currentItem.IsDeleted = false;
                    currentItem.Quantity = Quantity;
                }
                else
                {
                    currentItem.Quantity += Quantity;
                }
                currentItem.TotalPrice = PriceCalculatorHandler.GetInstance().CalculateItemPrice(
                    productDomainEntity.Sku,
                    currentItem.Quantity,
                    productDomainEntity.UnitPrice);
                _itemRepository.Update(currentItem);
            }
            
            await _itemRepository.SaveAsync();

            return true;
        }

        public async Task<List<Models.Item>> GetItemsByShoppingCartByProductId(int ShoppingCartId, int ProductId)
        {
            List<Models.Item> itemDomainEntity = new List<Models.Item>();
            
            var itemDataEntity = await _itemRepository.GetAllAsync();
            var items = itemDataEntity.Where(item => item.IdShoppingCart == ShoppingCartId && item.IdProduct == ProductId);

            var productReferenceDataEntity = await _productService.GetProductById(ProductId);
            foreach (var dataProduct in items)
            {
                Models.Item itemProduct = _mapper.Map<Models.Item>(dataProduct);
                Models.Product productReference = _mapper.Map<Models.Product>(productReferenceDataEntity);
                itemProduct.ProductReference = productReference;
                itemDomainEntity.Add(itemProduct);
            }
            return itemDomainEntity;
        }

        public async Task<List<Models.Item>> GetAllItems()
        {
            List<Models.Item> itemDomainEntity = new List<Models.Item>();

            var itemDataEntity = await _itemRepository.GetAllAsync();
            var items = itemDataEntity.Where(item => !item.IsDeleted);
            

            if (items != null)
            {
                foreach (var dataProduct in items)
                {
                    var productReferenceDataEntity = await _productService.GetProductById(dataProduct.IdProduct);
                    Models.Product productReference = _mapper.Map<Models.Product>(productReferenceDataEntity);
                    Models.Item itemProduct = _mapper.Map<Models.Item>(dataProduct);
                    itemProduct.ProductReference = productReference;
                    itemDomainEntity.Add(itemProduct);
                }
            }
            
            return itemDomainEntity;
        }
    }
}
