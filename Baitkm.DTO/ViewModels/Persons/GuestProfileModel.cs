using Baitkm.DTO.ViewModels.Bases;

namespace Baitkm.DTO.ViewModels.Persons
{
    public class GuestProfileModel : IViewModel
    {
        public int SaveFilterCount { get; set; }
        public int FavoriteCount { get; set; }
        public int CurrencyId { get; set; }
    }
}
