using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.FullName)
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(u => u.ProfilePhoto)
                .HasMaxLength(1000);
            builder.Property(u => u.PhoneCode)
                .HasMaxLength(10);
            builder.Property(u => u.Phone)
                .HasMaxLength(100);
            builder.Property(u => u.Email)
                .HasMaxLength(100);
            builder.Property(u => u.ForgotKey)
                .HasMaxLength(100);

            builder.HasOne(u => u.City)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CityId);

            builder.HasMany(u => u.Announcements)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

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
