using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers;
using System;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class FeaturedListModel : IViewModel
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public ImageOptimizer UserProfilePhoto { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public ImageOptimizer Photo { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}
