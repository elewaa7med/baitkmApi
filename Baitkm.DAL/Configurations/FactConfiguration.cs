using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class FactConfiguration : IEntityTypeConfiguration<Fact>
    {
        public void Configure(EntityTypeBuilder<Fact> builder)
        {
            builder.Property(p => p.ActivityType)
                .IsRequired();
            builder.Property(p => p.UserId)
                .IsRequired();
            builder.Property(p => p.IsGuest)
                .IsRequired();
            builder.Property(p => p.AnnouncementPhoto)
                .HasMaxLength(500);

            builder.HasOne(f => f.Announcement)
                .WithMany(a => a.Facts)
                .HasForeignKey(f => f.AnnouncementId)
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