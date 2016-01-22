using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace train_tickets_system.Models
{
    public class Price
    {
        // Fields
        public int PriceId { get; set; }
        public String Name { get; set; }
        public decimal Value { get; set; }

        // Constructors
        public Price()
        {
        }

        public Price(int id, string name, decimal value)
        {
            PriceId = id;
            this.Name = name;
            this.Value = value;
        }
    }
}
