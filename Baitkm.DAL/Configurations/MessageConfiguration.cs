using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasOne(m => m.User)
                .WithMany(u => u.Messages)
                .HasForeignKey(a => a.SenderId);

            builder.HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(a => a.ConversationId);

            builder.Property(m => m.MessageText)
                .HasMaxLength(1000);
            builder.Property(m => m.MessageBodyType)
                .IsRequired();
            builder.Property(m => m.IsSeen)
               .IsRequired();
            builder.Property(m => m.SenderMessageIsDeleted)
               .IsRequired();
            builder.Property(m => m.ReciverMessageIsDeleted)
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