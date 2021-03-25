using Baitkm.DTO.ViewModels;
using Baitkm.DTO.ViewModels.ForgotPasswords;
using Baitkm.DTO.ViewModels.Helpers;
using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.DTO.ViewModels.Persons;
using Baitkm.DTO.ViewModels.Persons.Users;
using Baitkm.DTO.ViewModels.Persons.Users.Verification;
using Baitkm.DTO.ViewModels.Subscription;
using Baitkm.DTO.ViewModels.Token;
using Baitkm.Entities;
using Baitkm.Enums;
using Baitkm.Enums.Notifications;
using Baitkm.Enums.UserRelated;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Users
{
    public interface IUserService
    {
        Task<TokenResponse> RegisterAsync(RegisterModel model, string deviceToken, OsType osType,
            Language language, string deviceId, string currencyCode);
        Task<bool> EditAsync(UserEditModel model, int userId, Language language);
        Task<UserDetailsModel> UserDetailsAsync(int userId, Language language);
        Task<UserProfileModel> UserProfileAsync(int userId);
        Task<User> GetUser(string userName, VerifiedBy verifiedBy);
        Task<UserDetailsAdminModel> GetByIdAsync(int userId);
        Task<PagingResponseModel<UserViewModel>> GetUserListAsync(PagingRequestModel model);
        Task<ImageOptimizer> Photo(UploadFileModel model, int userId);
        Task<bool> EditSubscription(UpdateSubscriptionModel model, int userId);
        Task<UpdateSubscriptionModel> GetSubscription(int userId);
        Task<bool> ChangePasswordAsync(ChangePasswordModel model, int userId, Language language);
        Task<bool> ForgotPasswordCheckVerified(SendKeyForgotPasswordModel model, Language language);
        Task<bool> ForgotPasswordChangeVerified(CheckForgotKeyModel model, Language language);
        Task<bool> ForgotPasswordChangePassword(ForgotPasswordChangePasswordModel model, Language language);
        Task<bool> Logout(string deviceId, OsType osType);
        Task<string> Block(int userId, int day);
        Task<bool> UnBlock(int userId);
        //Task<bool> Delete(int userId, string userName, Language language); //Will test
        Task<PagingResponseModel<UserViewModel>> UserFilter(UserFilterModel model, Language language, int userId);
        Task<bool> ResetPassword(UserResetPasswordModel model, int userId, Language language, int callerId);
        Task<PagingResponseModel<PersonNotificationModel>> NotificationList(PagingRequestModel model, int userId, Language language);
        Task<UserCurrencyResponseModel> GetUserCurrency(int userId);

        User CheckExistUser(int userId);
    }
}