using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class AnnouncementReportConfiguration : IEntityTypeConfiguration<AnnouncementReport>
    {
        public void Configure(EntityTypeBuilder<AnnouncementReport> builder)
        {
            builder.Property(ar => ar.ReportAnnouncementStatus)
                .IsRequired();
            builder.Property(ar => ar.Description)
                .HasMaxLength(1000)
                .IsRequired();

            builder.HasOne(ar => ar.Announcement)
               .WithMany(a => a.AnnouncementReports)
               .HasForeignKey(ar => ar.AnnouncementId);
            builder.HasOne(ar => ar.User)
               .WithMany(u => u.AnnouncementReports)
               .HasForeignKey(ar => ar.UserId);

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