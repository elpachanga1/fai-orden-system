using BusinessRules.RulesForPrice.Handlers;

namespace BusinessRules.RulesForPrice;

public class RuleWeighBased : IRulesForPrice
{
    public float CalculatePrice(float quantity, float price)
    {
        float priceKl = price * 1000;
        return (quantity * priceKl);
    }
}
