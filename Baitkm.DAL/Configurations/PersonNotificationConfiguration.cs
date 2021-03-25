using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class PersonNotificationConfiguration : IEntityTypeConfiguration<PersonNotification>
    {
        public void Configure(EntityTypeBuilder<PersonNotification> builder)
        {
            builder.Property(pn => pn.IsSeen)
                .IsRequired();

            builder.HasOne(pn => pn.User)
                .WithMany(u => u.PersonNotifications)
                .HasForeignKey(pn => pn.UserId);
            builder.HasOne(pn => pn.Guest)
                .WithMany(g => g.PersonNotifications)
                .HasForeignKey(pn => pn.GuestId);
            builder.HasOne(pn => pn.Announcement)
                .WithMany(a => a.PersonNotifications)
                .HasForeignKey(pn => pn.AnnouncementId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(pn => pn.Notification)
                .WithMany(n => n.PersonNotifications)
                .HasForeignKey(pn => pn.NotificationId);
            builder.HasOne(pn => pn.PushNotification)
                .WithMany(n => n.PersonNotifications)
                .HasForeignKey(pn => pn.PushNotificationId);

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