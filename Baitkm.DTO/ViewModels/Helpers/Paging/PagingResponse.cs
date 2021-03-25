using Baitkm.DTO.ViewModels.Bases;
using System;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Helpers.Paging
{
    public class PagingResponseModel<T> where T : class, IViewModel
    {
        public IEnumerable<T> Data { get; set; }
        public int ItemCount { get; set; }
        public int PageCount { get; set; }
        public DateTime? DateFrom { get; set; }
    }
}
