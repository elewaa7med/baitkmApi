using Baitkm.DTO.ViewModels.Announcements;
using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.Enums.Conversations;
using System;

namespace Baitkm.DTO.ViewModels.Conversations.SupportConversations.SupportMessages
{
    public class SupportMessageListModel : IViewModel
    {
        public int ConversationId { get; set; }
        public DateTime CreatedDate { get; set; }
        public ImageOptimizer Photo { get; set; }
        public int SenderId { get; set; }
        public string FullName { get; set; }
        public int MessageId { get; set; }
        public string MessageText { get; set; }
        public SupportMessageBodyType MessageBodyType { get; set; }
        public AnnouncementListViewModel Announcement { get; set; }
        public bool IsSentFromMe { get; set; }
        public string FileUrl { get; set; }
        public long FileSize { get; set; }
        public SupportMessageListModel ReplayMessage { get; set; }
    }
}
