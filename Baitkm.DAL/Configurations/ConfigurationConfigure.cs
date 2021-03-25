using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class ConfigurationConfigure : IEntityTypeConfiguration<Configuration>
    {
        public void Configure(EntityTypeBuilder<Configuration> builder)
        {
            builder.Property(conf => conf.Key)
                .IsRequired();
            builder.Property(conf => conf.Value)
                .IsRequired();
            builder.Property(conf => conf.Language)
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