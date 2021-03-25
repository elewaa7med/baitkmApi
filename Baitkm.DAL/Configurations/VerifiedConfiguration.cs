using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class VerifiedConfiguration : IEntityTypeConfiguration<Verified>
    {
        public void Configure(EntityTypeBuilder<Verified> builder)
        {
            builder.Property(v => v.PhoneCode)
                .HasMaxLength(50);
            builder.Property(v => v.Phone)
                .HasMaxLength(50);
            builder.Property(v => v.Email)
                .HasMaxLength(50);
            builder.Property(v => v.Code)
                .HasMaxLength(50);

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