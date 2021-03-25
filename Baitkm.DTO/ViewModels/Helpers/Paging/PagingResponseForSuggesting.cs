using Baitkm.DTO.ViewModels.Bases;

namespace Baitkm.DTO.ViewModels.Helpers.Paging
{
    public class PagingResponseForSuggesting<T> : PagingResponseModel<T> where T :  class, IViewModel
    {
        public bool IsSaveFilter { get; set; }
    }
}
