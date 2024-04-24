using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wheelzytest.Domain.DTOs
{
    public class CarInfoDto
    {
        public int Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Submodel { get; set; }
        public string BuyerName { get; set; }
        public decimal QuoteAmount { get; set; }
        public string StatusName { get; set; }
        public DateTime? StatusDate { get; set; }
    }
}
