using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Enums.Notifications;
using System;

namespace Baitkm.DTO.ViewModels.PushNotifications
{
    public class CreatePushNotificationModel : IViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ArabianTitle { get; set; }
        public string ArabianDescription { get; set; }
        public DateTime SendingDate { get; set; }
        public int? AnnouncementId { get; set; }
        public PushNotificationUserType? PushNotificationUserType { get; set; }
        public PushNotificationActionType PushNotificationActionType { get; set; }
    }
}