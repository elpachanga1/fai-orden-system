using BusinessRules.RulesForPrice.Handlers;

namespace BusinessRules.RulesForPrice;

public class RuleNormal : IRulesForPrice
{
    public float CalculatePrice(float quantity, float price)
    {
        return (price * quantity);
    }
}
