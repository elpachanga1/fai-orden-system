namespace Validations.Model
{
    public class Request
    {
        public required string UserName {get; set;}
        public DateTime ProcessCreationDate { get; set; }
        public List<ValidationMap> ValidationMaps { get; set; } = new List<ValidationMap>();
        public string? RecoveryNextHandlerName { get; set; }
        
}
}
