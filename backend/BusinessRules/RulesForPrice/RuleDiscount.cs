using BusinessRules.RulesForPrice.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessRules.RulesForPrice
{
    public class RuleDiscount : IRulesForPrice
    {
        /// <summary>
        /// Máximo porcentaje de descuento 50%
        /// Descuento por cada 3 unidades
        /// 20% de Descuento por cada 3 unidades
        /// </summary>
        private const float MaxDiscountPercentage = 0.5f; 
        private const float DiscountStep = 0.2f; 
        private const int UnitsPerDiscount = 3; 

        public float CalculatePrice(float quantity, float price)
        {
            float totalPrice = CalculateTotalPrice(quantity, price);
            float discountPercentage = CalculateDiscountPercentage(quantity);
            return ApplyDiscount(totalPrice, discountPercentage);
        }

        private float CalculateTotalPrice(float quantity, float price)
        {
            return quantity * price;
        }

        private float CalculateDiscountPercentage(float quantity)
        {
            int discountTimes = (int)quantity / UnitsPerDiscount;
            return Math.Min(discountTimes * DiscountStep, MaxDiscountPercentage);
        }

        private float ApplyDiscount(float totalPrice, float discountPercentage)
        {
            return totalPrice * (1 - discountPercentage);
        }
    }
}
