using Baitkm.DTO.ViewModels.Bases;

namespace Baitkm.DTO.ViewModels
{
    public class EmailModel : IViewModel
    {
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
