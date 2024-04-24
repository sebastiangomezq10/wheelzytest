using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wheelzytest.Domain.Models;

namespace wheelzytest.Infrastructure.ModelConfigurations
{
    public class BuyerConfiguration : IEntityTypeConfiguration<Buyer>
    {
        public void Configure(EntityTypeBuilder<Buyer> builder)
        {
            builder.HasKey(b => b.BuyerId);
            builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
        }
    }
}
