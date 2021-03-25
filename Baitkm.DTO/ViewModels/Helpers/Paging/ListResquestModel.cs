using Baitkm.DTO.ViewModels.Bases;

namespace Baitkm.DTO.ViewModels.Helpers.Paging
{
    public class ListResquestModel : IViewModel
    {
        public int? CurrencyId { get; set; }
        public PagingRequestModel PagingRequestModel { get; set; }
    }
}
