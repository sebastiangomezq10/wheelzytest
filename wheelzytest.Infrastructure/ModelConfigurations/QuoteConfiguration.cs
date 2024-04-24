using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wheelzytest.Domain.Models;

namespace wheelzytest.Infrastructure.ModelConfigurations
{
    public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
    {
        public void Configure(EntityTypeBuilder<Quote> builder)
        {
            builder.HasKey(q => q.QuoteId);
            builder.Property(q => q.Amount).HasColumnType("decimal(10, 2)");
            builder.HasOne(q => q.Car).WithMany(c => c.Quotes).HasForeignKey(q => q.CarId);
            builder.HasOne(q => q.Buyer).WithMany(b => b.Quotes).HasForeignKey(q => q.BuyerId);
            builder.Property(q => q.IsCurrent).IsRequired();
        }
    }
}
