using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class SaveFilterConfiguration : IEntityTypeConfiguration<SaveFilter>
    {
        public void Configure(EntityTypeBuilder<SaveFilter> builder)
        {
            builder.Property(usf => usf.Search)
                .HasMaxLength(100);
            builder.Property(usf => usf.SaveFilterName)
                .HasMaxLength(100);
            //builder.Property(usf => usf.Description)
            //    .HasMaxLength(100);

            builder.HasOne(sf => sf.User)
                .WithMany(a => a.SaveFilters)
                .HasForeignKey(sf => sf.UserId);
            builder.HasOne(sf => sf.Guest)
                .WithMany(a => a.SaveFilters)
                .HasForeignKey(sf => sf.GuestId);


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