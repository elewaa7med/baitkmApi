using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums.Notifications;

namespace Baitkm.DTO.ViewModels.Notifications
{
    public class NewMessageNotificationModel : IIndividualNotificationBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public NotificationType NotificationType { get; set; }
        public int GenericId { get; set; }
        public int? SenderId { get; set; }
        public int ReceiverId { get; set; }
        public int UnreadConversationCount { get; set; }
        public bool FromAdmin { get; set; }
    }
}