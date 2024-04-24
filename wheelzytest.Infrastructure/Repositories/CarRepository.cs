using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using wheelzytest.Domain.DTOs;
using wheelzytest.Domain.Interfaces;
using wheelzytest.Domain.Models;
using wheelzytest.Infrastructure.Context;

namespace wheelzytest.Infrastructure.Repositories
{


    // Implementación del repositorio
    public class CarRepository : ICarRepository
    {
        private readonly AppDbContext _context;

        public CarRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CarInfoDto>> GetCarInformationAsync()
        {
            var cars = await _context.Cars.ToListAsync();

            var currentQuotes = await _context.Quotes
                .Where(q => q.IsCurrent)
                .Select(q => new { q.CarId, q.Buyer.Name, q.Amount })
                .ToListAsync();

            var latestStatuses = await _context.Statuses
               .GroupBy(s => s.CarId)
               .Select(g => new { g.Key, Status = g.OrderByDescending(s => s.StatusId).FirstOrDefault() })
               .ToListAsync();

            var result = cars
                  .Select(c => new CarInfoDto
                  {
                      Year = c.Year,
                      Make = c.Make,
                      Model = c.Model,
                      Submodel = c.Submodel,
                      BuyerName = currentQuotes.FirstOrDefault(q => q.CarId == c.CarId).Name,
                      QuoteAmount = currentQuotes.FirstOrDefault(q => q.CarId == c.CarId).Amount,
                      StatusName = latestStatuses.FirstOrDefault(ls => ls.Key == c.CarId).Status.StatusName,
                      StatusDate = latestStatuses.FirstOrDefault(ls => ls.Key == c.CarId).Status.StatusDate
                  });

            return result;
        }

    }
}
