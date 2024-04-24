using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wheelzytest.Domain.Models
{
    public class Buyer
    {
        public int BuyerId { get; set; }
        public string Name { get; set; }

        // Relación con Quote
        public ICollection<Quote> Quotes { get; set; }
    }
}
