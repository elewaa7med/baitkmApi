using Baitkm.DTO.ViewModels.Persons.Users.Verification;
using Baitkm.DTO.ViewModels.Social;
using Baitkm.DTO.ViewModels.Token;
using Baitkm.Enums;
using Baitkm.Enums.Notifications;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Token
{
    public interface ITokenService
    {
        //refactored
        //Task<bool> SendKey(SendKeyModel model, Language language);
        //Task<bool> Verify(TokenVerifyViewModel model, string deviceToken, OsType osType, Language language, string deviceId);
        //Task<TokenResponse> Token(TokenViewModel model, string deviceToken, OsType osType, Language language, string deviceId);
        Task<TokenResponse> TokenAsync(TokenViewModel model, string deviceToken, OsType osType, Language language, string deviceId);
        //refactored
        Task<bool> VerifyAsync(TokenVerifyViewModel model, string deviceToken, OsType osType, Language language, string deviceId);
        //refactored
        Task<bool> SendKeyAsync(SendKeyModel model, Language language);

        Task<TokenResponse> SocialLogin(SocialLoginModel model,
            string deviceToken, Language language, OsType osType, string deviceId, string currencyCode);
        Task<TokenResponse> AppleLogin(AppleSignInRequestModel model,
            string email, string appleId, string deviceToken, Language language, string deviceId, string currencyCode);

        bool TokenTest();
        string ReturnString();
    }
}