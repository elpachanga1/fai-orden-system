namespace Services.Domain.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string UserName { get; set; }
        public required string Name { get; set; }
        public required string Password { get; set; }
        public Session? SessionReference { get; set; }
    }
}
