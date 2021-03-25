using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class PersonOtherSettingConfiguration : IEntityTypeConfiguration<PersonOtherSetting>
    {
        public void Configure(EntityTypeBuilder<PersonOtherSetting> builder)
        {
            builder.Property(pos => pos.AreaUnit)
                .IsRequired();
            builder.Property(pos => pos.Language)
                .IsRequired();

            builder.HasOne(pos => pos.User)
                .WithOne(u => u.PersonOtherSetting)
                .HasForeignKey<PersonOtherSetting>(pos => pos.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(pos => pos.Guest)
                .WithOne(g => g.PersonOtherSetting)
                .HasForeignKey<PersonOtherSetting>(pos => pos.GuestId)
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