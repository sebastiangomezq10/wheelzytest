using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wheelzytest.Domain.Models
{
    public class Quote
    {
        public int QuoteId { get; set; }
        public int CarId { get; set; }
        public int BuyerId { get; set; }
        public decimal Amount { get; set; }
        public bool IsCurrent { get; set; }

        // Relaciones con Car y Buyer
        public Car Car { get; set; }
        public Buyer Buyer { get; set; }
    }
}
