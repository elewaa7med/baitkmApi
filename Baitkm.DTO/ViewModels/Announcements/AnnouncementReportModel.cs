using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.Enums.Attachments;
using System;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class AnnouncementReportModel : IViewModel
    {
        public int Id { get; set; }
        public int AnnouncementId { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }
        public decimal Price { get; set; }
        public string Address { get; set; }
        public string Title { get; set; }
        public AnnouncementStatus AnnouncementStatus { get; set; }
        public AnnouncementStatus ReportStatus { get; set; }
        public AnnouncementRentType? AnnouncementRentType { get; set; }
        public ImageOptimizer UserProfilePhoto { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public ImageOptimizer Photo { get; set; }
        public string ShareUrl { get; set; }
        public int Rating { get; set; }
    }
}
