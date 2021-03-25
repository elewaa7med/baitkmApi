using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums.Notifications;
using Baitkm.Infrastructure.Validation.Attributes;

namespace Baitkm.DTO.ViewModels.PushNotifications
{
    public class FirebaseIndividualNotificationModel : IIndividualNotificationBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public NotificationType NotificationType { get; set; }
        public int GenericId { get; set; }
        public PushNotificationActionType PushNotificationActionType { get; set; }
        public int UnreadConversationCount { get; set; }
        public bool FromAdmin { get; set; }
        public int? SenderId { get; set; }
        public int ReceiverId { get; set; }
    }
}