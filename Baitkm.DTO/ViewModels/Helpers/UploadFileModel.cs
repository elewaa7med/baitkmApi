using Baitkm.DTO.ViewModels.Bases;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Baitkm.DTO.ViewModels.Helpers
{
    public class UploadFileModel : IViewModel
    {
        [Required]
        public IFormFile File { get; set; }
    }
}