namespace Services.Domain.Services
{
    public class StoreService
    {
        private readonly ProductService _productService;
        private readonly UserService _userService;
        
        public StoreService(
            ProductService productService,
            UserService userService,
            ItemService itemService
        ) {
            _productService = productService;
            _userService = userService;            
        }

        public virtual async Task<IEnumerable<Models.Product>> GetAllProducts()
        {
            return await _productService.GetAllProducts();
        }

        public virtual async Task<Models.Product?> GetProductById(int Id)
        {
            return await _productService.GetProductById(Id);
        }

        public virtual async Task<bool> AddProduct(string Sku, string Name, string? Description, int AvailableUnits, float UnitPrice, string? Image)
        {
            return await _productService.AddProduct(Sku, Name, Description, AvailableUnits, UnitPrice, Image);
        }

        public virtual async Task<bool> AddProductToShoppingCart(string IdUser, int IdProduct, int Quantity)
        {
            bool result = await _userService.AddProductToShoppingCart(IdUser, IdProduct, Quantity);
            return result;
        }

        public virtual async Task<bool> DeleteProductFromShoppingCart(string IdUser, int IdItem)
        {
            bool result = await _userService.DeleteProductFromShoppingCart(IdUser, IdItem);
            return result;
        }

        public virtual async Task<bool> EmptyShoppingCart(string IdUser)
        {
            bool result = await _userService.EmptyShoppingCart(IdUser);
            return result;
        }

        public virtual async Task<bool> CompleteshoppingCart(string IdUser)
        {
            bool result = await _userService.CompleteShoppingCart(IdUser);
            return result;
        }

        public virtual async Task<IEnumerable<Domain.Models.Item>> GetItemsByProductId(int ProductId)
        {
            return await _userService.GetItemsByProductId(ProductId);
        }

        public virtual async Task<IEnumerable<Domain.Models.Item>> GetAllItems()
        {
            return await _userService.GetAllItems();            
        }

        public virtual async Task<float> GetTotalSales()
        {
            Domain.Models.Store store = new Models.Store();
            store.TotalSales = await _userService.GetTotalSales();
            return store.TotalSales;
        }

        public virtual async Task<Models.User?> AuthenticateUser(string username, string password)
        {
            return await _userService.AuthenticateUser(username, password);
        }
    }
}
