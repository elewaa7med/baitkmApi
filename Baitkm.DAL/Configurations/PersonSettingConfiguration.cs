using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class PersonSettingConfiguration : IEntityTypeConfiguration<PersonSetting>
    {
        public void Configure(EntityTypeBuilder<PersonSetting> builder)
        {
            builder.Property(s => s.SubscriptionsType)
                .IsRequired();

            builder.HasOne(s => s.User)
                .WithMany(u => u.PersonSettings)
                .HasForeignKey(s => s.UserId);
            builder.HasOne(s => s.Guest)
                .WithMany(u => u.PersonSettings)
                .HasForeignKey(s => s.GuestId);

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