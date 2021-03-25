using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums.Notifications;
using Baitkm.Infrastructure.Validation.Attributes;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.PushNotifications
{
    public class FirebaseCampaignNotificationModel : IGroupNotificationBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public NotificationType NotificationType { get; set; }
        public int GenericId { get; set; }
        public PushNotificationActionType PushNotificationActionType { get; set; }
        public int UnreadConversationCount { get; set; }
        public bool FromAdmin { get; set; }
        [PropertyNotMapped]
        public int? SenderId { get; set; }
        [PropertyNotMapped]
        public List<int> UserIds { get; set; }
        [PropertyNotMapped]
        public List<int> GuestIds { get; set; }
    }
}