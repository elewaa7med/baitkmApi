using Baitkm.DTO.ViewModels.Cities;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.SaveFilters
{
    public class SaveFilterViewModel : AddSaveFilterViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int FilterCount { get; set; }
        public CityViewModel City { get; set; }
        public CountryResponseModel Country { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
    }
}