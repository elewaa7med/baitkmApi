using Baitkm.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Baitkm.DAL.Configurations
{
    internal class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            builder.Property(p => p.AnnouncementCreatorConversationIsDeleted)
                .IsRequired();
            builder.Property(p => p.QuestionerConversationIsDeleted)
                .IsRequired();

            builder.HasOne(c => c.Announcement)
                .WithMany(a => a.Conversations)
                .HasForeignKey(c => c.AnnouncementId);

            builder.HasOne(c => c.AnnouncementCreator)
                .WithMany(u => u.CreatedConversations)
                .HasForeignKey(c => c.AnnouncementCreatorId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(c => c.Questioner)
                .WithMany(u => u.QuestionedConversations)
                .HasForeignKey(c => c.QuestionerId)
                .OnDelete(DeleteBehavior.Restrict);

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