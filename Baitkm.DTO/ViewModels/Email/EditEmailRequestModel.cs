using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.Email
{
    public class EditEmailRequestModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
