namespace Baitkm.MediaServer.BLL.ViewModels
{
    public class DownloadFileModel : IViewModel
    {
        public byte[] Bytes { get; set; }
        public string ContentType { get; set; }
    }
}
