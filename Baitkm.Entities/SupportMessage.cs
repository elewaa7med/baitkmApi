using Baitkm.Entities.Base;
using Baitkm.Enums.Conversations;

namespace Baitkm.Entities
{
    public class SupportMessage : EntityBase
    {
        public int SupportConversationId { get; set; }
        public int? UserSenderId { get; set; }
        public int? GuestSenderId { get; set; }
        public int? ReplayMessageId { get; set; }
        public string MessageText { get; set; }
        public SupportMessageBodyType SupportMessageBodyType { get; set; }
        public bool IsSeen { get; set; }
        public long FileLength { get; set; }

        public virtual SupportConversation SupportConversation { get; set; }
        public virtual User UserSender { get; set; }
        public virtual Guest GuestSender { get; set; }
    }
}