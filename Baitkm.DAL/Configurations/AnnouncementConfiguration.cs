using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
    {
        public void Configure(EntityTypeBuilder<Announcement> builder)
        {
            builder.HasOne(a => a.User)
                .WithMany(u => u.Announcements)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.Title)
                .HasMaxLength(1000);
            builder.Property(a => a.Description)
                .HasMaxLength(2000);
            builder.Property(a => a.TitleArabian)
                .HasMaxLength(1000);
            builder.Property(a => a.DescriptionArabian)
                .HasMaxLength(2000);
            builder.Property(a => a.AddressEn)
                .HasMaxLength(300);
            builder.Property(a => a.AddressAr)
                .HasMaxLength(300);
            builder.Property(a => a.District)
                .HasMaxLength(300);
            builder.Property(a => a.DisctrictName)
                .HasMaxLength(300);

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