using Baitkm.DTO.ViewModels.Bases;
using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class AddRatingModel : IViewModel
    {
        [Range(1, int.MaxValue)]
        public int AnnouncementId { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
    }
}