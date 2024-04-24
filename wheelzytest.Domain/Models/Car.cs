using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wheelzytest.Domain.Models
{
    public class Car
    {
        public int CarId { get; set; }
        public int Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Submodel { get; set; }
        public string ZipCode { get; set; }

        public ICollection<Quote> Quotes { get; set; }
        public ICollection<Status> Statuses { get; set; }
    }
}
