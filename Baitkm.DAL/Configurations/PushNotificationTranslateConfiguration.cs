using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class PushNotificationTranslateConfiguration : IEntityTypeConfiguration<PushNotificationTranslate>
    {
        public void Configure(EntityTypeBuilder<PushNotificationTranslate> builder)
        {
            builder.Property(p => p.Title)
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(p => p.Description)
                .HasMaxLength(1000)
                .IsRequired();
            builder.Property(p => p.Language)
                .IsRequired();

            builder.HasOne(pnt => pnt.PushNotification)
                .WithMany(pn => pn.PushNotificationTranslates)
                .HasForeignKey(pnt => pnt.PushNotificationId);

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