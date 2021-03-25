using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    public class NotificationTranslateConfiguration : IEntityTypeConfiguration<NotificationTranslate>
    {
        public void Configure(EntityTypeBuilder<NotificationTranslate> builder)
        {
            builder.Property(nt => nt.Title)
                .HasMaxLength(500)
                .IsRequired();
            builder.Property(nt => nt.Description)
                .HasMaxLength(1000)
                .IsRequired();
            builder.Property(nt => nt.Language)
                .IsRequired();

            builder.HasOne(nt => nt.Notification)
                .WithMany(n => n.NotificationTranslate)
                .HasForeignKey(nt => nt.NotificationId);

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