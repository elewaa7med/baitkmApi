using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums.Notifications;
using System;

namespace Baitkm.DTO.ViewModels.PushNotifications
{
    public class UpdatePushNotificationModel : IViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime SendingDate { get; set; }
        public int? AnousmentId { get; set; }
        public PushNotificationUserType PushNotificationUserType { get; set; }
        public PushNotificationStatusType PushNotificationStatusType { get; set; }
        public PushNotificationActionType PushNotificationActionType { get; set; }
    }
}
