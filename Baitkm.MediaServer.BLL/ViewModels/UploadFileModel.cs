using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Baitkm.MediaServer.BLL.ViewModels
{
    public class UploadFileModel : IViewModel
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
