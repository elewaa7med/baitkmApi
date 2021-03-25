using Baitkm.Infrastructure.Helpers.Models;
using Quartz;
using System.Collections.Concurrent;
using System.Collections.Specialized;

namespace Baitkm.Infrastructure.Constants
{
    public class ConstValues
    {
        public static string ConnectionString { get; set; }
        public static string TwilioAccountSid { get; set; }
        public static string TwilioAuthToken { get; set; }
        public static string TwilioPhone { get; set; }
        public static string EmailAccount { get; set; }
        public static string EmailAccountPassword { get; set; }
        public static string MediaBaseUrl { get; set; }
        public static string MediaResize { get; set; } = "api/Image/Resize/";
        public static string MediaDownload { get; set; } = "api/Image/Download/";
        public static string MediaFromSocialPage { get; set; } = "api/Image/DownloadFromSocialPage/";
        public static string MediaUpload { get; set; } = "api/Image/Upload/";
        public static string MediaRemove { get; set; } = "api/Image/Remove/";
        public static string MediaLength { get; set; } = "api/Image/GetLength/";
        public static string MediaMultipleUpload { get; set; } = "api/Image/UploadMultiple/";
        public static string DownloadMap { get; set; } = "api/Image/DownloadMap/";
        public static string AppleSocialLoginUrl { get; set; }
        public static string AppleAuthorize { get; set; }
        public static string GoogleSocialLoginUrl { get; set; }
        public static string FacebookPersonInfoUrl { get; set; }
        public static string FacebookGraphBaseUrl { get; } = "https://graph.facebook.com/";
        public static string GoogleLocateBase { get; set; }
        public static string GoogleMapsBase { get; set; }
        public static string GoogleLocateKey { get; set; }
        public static string FirebaseServerKey { get; set; }
        public static string FirebaseSenderId { get; set; }
        public static string FirebaseUrl { get; set; }
        public static string SlackUrl { get; set; }
        public static string BackEndBaseUrl { get; set; }
        public static NameValueCollection QuartzConfigs { get; set; }
        public static IScheduler Scheduler { get; set; }
        public static bool IsServerConnected { get; set; }
        public static int Width { get; set; }
        public static int Height { get; set; }
        public static bool IsDevelopmentEnvironment { get; set; }
        public static ConcurrentDictionary<int, ProgressHelperModel> ProgressModels { get; } = new ConcurrentDictionary<int, ProgressHelperModel>();
        public static string IpAddressUrl { get; set; }
    }
}