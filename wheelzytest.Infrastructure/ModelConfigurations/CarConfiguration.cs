using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wheelzytest.Domain.Models;

namespace wheelzytest.Infrastructure.ModelConfigurations
{

    public class CarConfiguration : IEntityTypeConfiguration<Car>
    {
        public void Configure(EntityTypeBuilder<Car> builder)
        {
            builder.HasKey(c => c.CarId);
            builder.Property(c => c.Make).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Model).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Submodel).HasMaxLength(100);
            builder.Property(c => c.ZipCode).IsRequired().HasMaxLength(10);
        }
    }
}
