using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wheelzytest.Domain.Models
{
    public class Status
    {
        public int StatusId { get; set; }
        public int CarId { get; set; }
        public string StatusName { get; set; }
        public DateTime? StatusDate { get; set; } 
        public string ChangedBy { get; set; }

        
        public Car Car { get; set; }
    }
}
