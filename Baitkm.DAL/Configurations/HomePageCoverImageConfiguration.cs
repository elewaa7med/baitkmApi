using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class HomePageCoverImageConfiguration : IEntityTypeConfiguration<HomePageCoverImage>
    {
        public void Configure(EntityTypeBuilder<HomePageCoverImage> builder)
        {
            builder.Property(h => h.Photo)
                .HasMaxLength(1000)
                .IsRequired();
            builder.Property(h => h.IsBase)
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