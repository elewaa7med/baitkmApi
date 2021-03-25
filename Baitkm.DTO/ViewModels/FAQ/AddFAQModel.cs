using Baitkm.DTO.ViewModels.Bases;

namespace Baitkm.DTO.ViewModels.FAQ
{
    public class AddFAQModel : IViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
