using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class ViewedAnnouncementConfiguration : IEntityTypeConfiguration<ViewedAnnouncement>
    {
        public void Configure(EntityTypeBuilder<ViewedAnnouncement> builder)
        {
            builder.HasOne(va => va.Announcement)
                .WithMany(a => a.ViewedAnnouncements)
                .HasForeignKey(va => va.AnnouncementId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(va => va.User)
                .WithMany(a => a.ViewedAnnouncements)
                .HasForeignKey(va => va.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(va => va.Guest)
                .WithMany(a => a.ViewedAnnouncements)
                .HasForeignKey(va => va.GuestId)
                .OnDelete(DeleteBehavior.Cascade);

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