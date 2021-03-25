using Baitkm.DTO.ViewModels.Bases;

namespace Baitkm.DTO.ViewModels.Announcements
{
    public class AddTitleDescriptionModel : IViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string TitleArabian { get; set; }
        public string DescriptionArabian { get; set; }
        public double DefaultDay { get; set; }
        public double Day { get; set; }
    }
}