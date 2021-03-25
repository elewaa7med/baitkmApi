using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class SubscribeAnnouncementConfiguration : IEntityTypeConfiguration<SubscribeAnnouncement>
    {
        public void Configure(EntityTypeBuilder<SubscribeAnnouncement> builder)
        {
            builder.Property(p => p.Email)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasOne(s => s.User)
                .WithMany(u => u.SubscribeAnnouncements)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(s => s.Guest)
                .WithMany(g => g.SubscribeAnnouncements)
                .HasForeignKey(s => s.GuestId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(s => s.Announcement)
                .WithMany(a => a.SubscribeAnnouncements)
                .HasForeignKey(s => s.AnnouncementId)
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