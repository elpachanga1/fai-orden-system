namespace DataRepository.Models
{
    public class Item
    {
        public int Id { get; set; }
        public int IdShoppingCart { get; set; }
        public int IdProduct { get; set; }
        public int Quantity { get; set; }
        public bool IsDeleted { get; set; }
        public float TotalPrice { get; set; }
        
    }
}
