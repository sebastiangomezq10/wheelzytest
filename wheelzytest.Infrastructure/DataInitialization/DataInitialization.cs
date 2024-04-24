using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wheelzytest.Domain.Models;
using wheelzytest.Infrastructure.Context;

namespace wheelzytest.Infrastructure.DataInitialization
{
    public static class DatabaseInitializer
    {
        public static void Initialize(AppDbContext context)
        {

            context.Database.EnsureCreated();


            if (!context.Cars.Any())
            {

                var cars = new List<Car>
                    {
                        new Car { Year = 2020, Make = "Toyota", Model = "Corolla", Submodel = "LE", ZipCode = "32801" },
                        new Car { Year = 2019, Make = "Honda", Model = "Civic", Submodel = "EX", ZipCode = "32801" }
                    };
                context.Cars.AddRange(cars);


                var buyers = new List<Buyer>
                    {
                        new Buyer { Name = "John Doe" },
                        new Buyer { Name = "Jane Smith" }
                    };
                context.Buyers.AddRange(buyers);


                context.SaveChanges();


                var quotes = new List<Quote>
                    {
                        new Quote { CarId = cars[0].CarId, BuyerId = buyers[0].BuyerId, Amount = 15000.00m, IsCurrent = true },
                        new Quote { CarId = cars[1].CarId, BuyerId = buyers[1].BuyerId, Amount = 14000.00m, IsCurrent = true }
                    };
                context.Quotes.AddRange(quotes);

                var statuses = new List<Status>
                    {
                        new Status { CarId = cars[0].CarId, StatusName = "Accepted", StatusDate = DateTime.Now, ChangedBy = "System" },
                        new Status { CarId = cars[1].CarId, StatusName = "Pending Acceptance", ChangedBy = "System" }
                    };
                context.Statuses.AddRange(statuses);


                context.SaveChanges();
            }
        }
    }
}
