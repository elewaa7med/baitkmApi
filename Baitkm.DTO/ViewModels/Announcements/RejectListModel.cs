using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.Enums.Attachments;
using System;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class RejectListModel : IViewModel
    {
        public int Id { get; set; }
        public AnnouncementType AnnouncementType { get; set; }
        public AnnouncementEstateType AnnouncementEstateType { get; set; }
        public AnnouncementStatus AnnouncementStatus { get; set; }
        public AnnouncementRentType? AnnouncementRentType { get; set; }
        public string Address { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public ImageOptimizer UserProfilePhoto { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public ImageOptimizer Photo { get; set; }
        public DateTime? RejectDate { get; set; }
    }
}
