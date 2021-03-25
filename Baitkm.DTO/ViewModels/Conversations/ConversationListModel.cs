using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.Enums.Conversations;
using System;

namespace Baitkm.DTO.ViewModels.Conversations
{
    public class ConversationListModel : IViewModel
    {
        public int ConversationId { get; set; }
        public int AnnouncementId { get; set; }
        public ImageOptimizer Photo { get; set; }
        public string FullName { get; set; }
        public DateTime MessageDate { get; set; }
        public string MessageText { get; set; }
        public MessageBodyType MessageBodyType { get; set; }
        public int UnSeenCount { get; set; }
    }
}
