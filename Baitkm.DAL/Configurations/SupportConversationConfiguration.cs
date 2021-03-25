using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class SupportConversationConfiguration : IEntityTypeConfiguration<SupportConversation>
    {
        public void Configure(EntityTypeBuilder<SupportConversation> builder)
        {
            builder.HasOne(sc => sc.Admin)
                .WithMany(u => u.SupportConversations)
                .HasForeignKey(sc => sc.AdminId);

            builder.HasOne(sc => sc.User)
                .WithOne(u => u.SupportConversation)
                .HasForeignKey<SupportConversation>(sc => sc.UserId);
            builder.HasOne(sc => sc.Guest)
                .WithOne(g => g.SupportConversation)
                .HasForeignKey<SupportConversation>(sc => sc.GuestId);

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