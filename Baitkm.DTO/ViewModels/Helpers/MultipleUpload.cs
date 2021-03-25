using Baitkm.DTO.ViewModels.Bases;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Baitkm.DTO.ViewModels.Helpers
{
    public class MultipleUpload : IViewModel
    {
        public int AnnouncementId { get; set; }
        public List<IFormFile> Photos { get; set; }
        public List<string> PhotoPaths { get; set; }
    }
}
