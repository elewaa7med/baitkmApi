using Baitkm.DTO.ViewModels.Bases;
using System;

namespace Baitkm.DTO.ViewModels.Helpers.Paging
{
    public class PagingRequestModel : IViewModel
    {
        public int Count { get; set; }
        public int Page { get; set; }
        public DateTime? DateFrom { get; set; }
    }
}
