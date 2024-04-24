using Microsoft.EntityFrameworkCore;
using wheelzytest.Domain.Models;
using wheelzytest.Infrastructure.ModelConfigurations;

namespace wheelzytest.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Car> Cars { get; set; }
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Status> Statuses { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CarConfiguration());
            modelBuilder.ApplyConfiguration(new BuyerConfiguration());
            modelBuilder.ApplyConfiguration(new QuoteConfiguration());
            modelBuilder.ApplyConfiguration(new StatusConfiguration());
        }
    }
}
