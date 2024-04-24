using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using wheelzytest.Domain.Models;

namespace wheelzytest.Infrastructure.ModelConfigurations
{
    public class StatusConfiguration : IEntityTypeConfiguration<Status>
    {
        public void Configure(EntityTypeBuilder<Status> builder)
        {
            builder.HasKey(s => s.StatusId);
            builder.Property(s => s.StatusName).IsRequired().HasMaxLength(100);
            builder.Property(s => s.ChangedBy).HasMaxLength(100);
            builder.HasOne(s => s.Car).WithMany(c => c.Statuses).HasForeignKey(s => s.CarId);
        }
    }
}
