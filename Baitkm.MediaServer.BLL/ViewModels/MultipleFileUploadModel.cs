using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Baitkm.MediaServer.BLL.ViewModels
{
    public class MultipleFileUploadModel : IViewModel
    {
        public List<IFormFile> Files { get; set; }
    }
}
