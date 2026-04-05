namespace Validations.Model
{
    public class ValidationMap
    {
        public required string ValidationName { get; set; }
        public bool State {  get; set; }
        public DateTime CreationDate { get; set; }
        public Dictionary<string, string> DetailData { get; set; } = new Dictionary<string, string>();
    }
}
