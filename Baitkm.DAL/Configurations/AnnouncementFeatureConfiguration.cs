using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class AnnouncementFeatureConfiguration : IEntityTypeConfiguration<AnnouncementFeature>
    {
        public void Configure(EntityTypeBuilder<AnnouncementFeature> builder)
        {
            builder.HasOne(af => af.Announcement)
                .WithMany(a => a.Features)
                .HasForeignKey(af => af.AnnouncementId);
            builder.Property(af => af.FeatureType)
                .IsRequired();

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