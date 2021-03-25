namespace Baitkm.MediaServer.BLL.ViewModels
{
    public class DownloadMapModel : IViewModel
    {
        public string Url { get; set; }
        public int AnnouncementId { get; set; }
        public bool IsRelativeRequested { get; set; }
    }
}
