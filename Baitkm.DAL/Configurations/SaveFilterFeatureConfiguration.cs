using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class SaveFilterFeatureConfiguration : IEntityTypeConfiguration<SaveFilterFeature>
    {
        public void Configure(EntityTypeBuilder<SaveFilterFeature> builder)
        {
            builder.HasOne(sfa => sfa.SaveFilter)
                .WithMany(sf => sf.Features)
                .HasForeignKey(sfa => sfa.SaveFilterId);
            builder.Property(sfa => sfa.FeatureType)
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