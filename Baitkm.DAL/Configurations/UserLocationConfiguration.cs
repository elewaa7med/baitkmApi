using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class UserLocationConfiguration : IEntityTypeConfiguration<UserLocation>
    {
        public void Configure(EntityTypeBuilder<UserLocation> builder)
        {
            builder.Property(ul => ul.Country)
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(ul => ul.City)
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(ul => ul.Address)
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(ul => ul.IpAddress)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasOne(ul => ul.User)
                .WithOne(u => u.UserLocation)
                .HasForeignKey<UserLocation>(ul => ul.UserId);

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
