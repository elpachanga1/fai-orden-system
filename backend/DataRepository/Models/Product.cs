namespace DataRepository.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int AvailableUnits { get; set; }
        public float UnitPrice { get; set; }
        public string? Image { get; set; }
    }
}


