namespace DataRepository.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string IdUser { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime FinishDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}
