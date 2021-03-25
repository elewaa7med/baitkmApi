using Baitkm.Enums;
using System;

namespace Baitkm.DTO.ViewModels
{
    public class ActivityResponseModel
    {
        public int AnnouncementId { get; set; }
        public ActivityType ActivityType { get; set; }
        public DateTime CreatedDt { get; set; }
        public string Photo { get; set; }
    }
}
