using Baitkm.DTO.ViewModels.Bases;
using Baitkm.DTO.ViewModels.Helpers;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Configurations
{
    public class GetHomePageListModel : IViewModel
    {
        public int Id { get; set; }
        public bool IsBase { get; set; }
        public ImageOptimizer Photo { get; set; }
    }
}