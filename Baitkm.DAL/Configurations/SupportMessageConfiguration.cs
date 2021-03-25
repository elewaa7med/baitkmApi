using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class SupportMessageConfiguration : IEntityTypeConfiguration<SupportMessage>
    {
        public void Configure(EntityTypeBuilder<SupportMessage> builder)
        {
            builder.Property(sm => sm.MessageText)
                .HasMaxLength(1000)
                .IsRequired();
            builder.Property(sm => sm.SupportMessageBodyType)
                .IsRequired();
            builder.Property(sm => sm.IsSeen)
               .IsRequired();

            builder.HasOne(sm => sm.SupportConversation)
                .WithMany(sc => sc.SupportMessages)
                .HasForeignKey(sm => sm.SupportConversationId);
            builder.HasOne(sm => sm.UserSender)
                .WithMany(u => u.SupportMessages)
                .HasForeignKey(sm => sm.UserSenderId);
            builder.HasOne(sm => sm.GuestSender)
                .WithMany(g => g.SupportMessages)
                .HasForeignKey(sm => sm.GuestSenderId);

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