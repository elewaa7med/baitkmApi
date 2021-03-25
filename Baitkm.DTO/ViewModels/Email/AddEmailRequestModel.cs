using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.Email
{
    public class AddEmailRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
