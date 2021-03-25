using Baitkm.Entities.Base;
using Baitkm.Enums.Conversations;

namespace Baitkm.Entities
{
    public class Message : EntityBase
    {
        public int SenderId { get; set; }
        public int ReciverId { get; set; }
        public int ConversationId { get; set; }
        public int? ReplayMessageId { get; set; }
        public string MessageText { get; set; }
        public MessageBodyType MessageBodyType { get; set; }
        public bool IsSeen { get; set; }
        public long FileLength { get; set; }
        public bool SenderMessageIsDeleted { get; set; }
        public bool ReciverMessageIsDeleted { get; set; }

        public virtual Conversation Conversation { get; set; }
        public virtual User User { get; set; }
    }
}
