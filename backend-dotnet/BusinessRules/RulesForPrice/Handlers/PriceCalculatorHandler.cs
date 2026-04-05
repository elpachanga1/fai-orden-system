using BusinessRules.RulesForPrice.Enums;

namespace BusinessRules.RulesForPrice.Handlers;

public class PriceCalculatorHandler 
{

    private static readonly PriceCalculatorHandler _instance = new PriceCalculatorHandler();
    
    //Clases de reglas
    private static readonly RuleNormal _normalPriceRule = new RuleNormal();
    private static readonly RuleWeighBased _weightBasedPriceRule = new RuleWeighBased();
    private static readonly RuleDiscount _specialDiscountPriceRule = new RuleDiscount();

    private PriceCalculatorHandler() { }

    public static PriceCalculatorHandler GetInstance()
    {
        return _instance;
    }

    public float CalculateItemPrice(string sku, float quantity, float price)
    {
        var pricingRule = GetPricingRule(sku);
        return pricingRule.CalculatePrice(quantity, price);
    }


    private IRulesForPrice GetPricingRule(string sku)
    {
        if (sku.StartsWith("EA"))
        {
            return _normalPriceRule;
        }
        else if (sku.StartsWith("WE"))
        {
            return _weightBasedPriceRule;
        }
        else if (sku.StartsWith("SP"))
        {
            return _specialDiscountPriceRule;
        }
        else
        {
            throw new ArgumentException("SKU Not Implemented");
        }
    }
}
