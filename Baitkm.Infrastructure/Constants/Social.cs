namespace Baitkm.Infrastructure.Constants
{
    public class Social
    {
        public const string GoogleSocialLoginUrl = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=";
        public const string FacebookPersonInfoUrl = "me?fields=last_name,first_name,email,picture.width(400)&access_token=";
        public static string FacebookGraphBaseUrl { get; } = "https://graph.facebook.com/";
    }
}
