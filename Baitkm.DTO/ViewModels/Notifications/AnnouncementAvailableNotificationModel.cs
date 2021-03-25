using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums.Notifications;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Notifications
{
    public class AnnouncementAvailableNotificationModel : IGroupNotificationBase
    {
        public List<int> UserIds { get; set; }
        public List<int> GuestIds { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public NotificationType NotificationType { get; set; }
        public int GenericId { get; set; }
        public int? SenderId { get; set; }
        public int UnreadConversationCount { get; set; }
        public bool FromAdmin { get; set; }
    }
}