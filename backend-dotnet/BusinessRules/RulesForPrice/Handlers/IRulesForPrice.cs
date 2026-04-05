namespace BusinessRules.RulesForPrice.Handlers;

public interface IRulesForPrice
{
    public float CalculatePrice(float quantity, float price);
}
