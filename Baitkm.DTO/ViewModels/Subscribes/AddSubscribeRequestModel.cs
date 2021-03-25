using Baitkm.DTO.ViewModels.Bases;
using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.Subscribes
{
    public class AddSubscribeRequestModel : IViewModel
    {
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }
        public int AnnouncementId { get; set; }
    }
}