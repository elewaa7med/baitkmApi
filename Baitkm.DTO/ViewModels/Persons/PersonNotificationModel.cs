using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.DTO.ViewModels.PushNotifications;
using System;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Persons
{
    public class PersonNotificationModel : IViewModel
    {
        public int? AnnouncementId { get; set; }
        public ImageOptimizer Photo { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime NotificationSendDate { get; set; }
    }
}
