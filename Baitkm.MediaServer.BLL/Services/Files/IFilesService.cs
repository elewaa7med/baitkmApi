using Baitkm.Enums.Attachments;
using Baitkm.MediaServer.BLL.ViewModels;
using System.Threading.Tasks;

namespace Baitkm.MediaServer.BLL.Services.Files
{
    public interface IFilesService
    {
        Task<string> FileUploader(UploadFileModel model, UploadType type, bool isRelativeRequested, int announcementId = 0);
        bool MultipleFileUploader(MultipleFileUploadModel model, UploadType type, AttachmentType announcementPhotoType, bool isRelativeRequested, int announcementId);
        bool RemoveFile(string fileName, UploadType type, int id);
        Task<DownloadFileModel> Download(UploadType type, string fileName, int id, bool isBlur);
        Task<DownloadFileModel> Resize(UploadType type, string fileName, int maxWidth, int maxHeight, int id, bool isBlur);
        string DownloadFromSocialPage(string uri);
        long GetLength(string fileName, UploadType uploadType, int id);
        string DownloadMap(DownloadMapModel model);
        //Task<string> DownloadMap(DownloadMapModel model);
    }
}
