using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasOne(ci => ci.Country)
                .WithMany(co => co.Cities)
                .HasForeignKey(ci => ci.CountryId);

            builder.HasKey(s => s.Id);
            builder.Property(p => p.CreatedDt)
                .IsRequired();
            builder.Property(p => p.UpdatedDt)
                .IsRequired();
            builder.Property(p => p.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);
        }
    }
}