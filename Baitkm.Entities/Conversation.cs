using Baitkm.Entities.Base;
using System.Collections.Generic;

namespace Baitkm.Entities
{
    public class Conversation : EntityBase
    {
        public int AnnouncementId { get; set; }
        public int AnnouncementCreatorId { get; set; }
        public int QuestionerId { get; set; }
        public bool AnnouncementCreatorConversationIsDeleted { get; set; }
        public bool QuestionerConversationIsDeleted { get; set; }

        public virtual Announcement Announcement { get; set; }
        public virtual User AnnouncementCreator { get; set; }
        public virtual User Questioner { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}