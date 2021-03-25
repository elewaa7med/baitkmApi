using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class AnnouncementAttachmentConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.Property(p => p.File)
                .HasMaxLength(1000)
                .IsRequired();
            builder.Property(p => p.AttachmentType)
                .IsRequired();

            builder.HasOne(aa => aa.Announcement)
                .WithMany(a => a.Attachments)
                .HasForeignKey(aa => aa.AnnouncementId)
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