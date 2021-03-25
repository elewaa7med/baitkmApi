using Baitkm.DTO.ViewModels.Announcements;
using Baitkm.DTO.ViewModels.Bases;

namespace Baitkm.DTO.ViewModels.SaveFilters
{
    public class AddSaveFilterViewModel : FilterAnnouncementModel, IViewModel
    {
        public string SaveFilterName { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
    }
}