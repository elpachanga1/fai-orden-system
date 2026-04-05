using DataRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Domain.Models
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

        public bool HasUnits()
        {
            return (AvailableUnits > 0);
        }

        public void DiscountUnits(int Quantity)
        {
            AvailableUnits = AvailableUnits - Quantity;
        }
            
    }
}
