using Baitkm.Enums.Notifications;

namespace Baitkm.DTO.ViewModels.Bases
{
    public interface INotificationBase
    {
        string Title { get; set; }
        string Description { get; set; }
        NotificationType NotificationType { get; set; }
        int GenericId { get; set; }
        int? SenderId { get; set; }
        int UnreadConversationCount { get; set; }
        bool FromAdmin { get; set; }
    }
}