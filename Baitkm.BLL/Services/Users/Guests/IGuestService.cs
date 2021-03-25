using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.DTO.ViewModels.Persons;
using Baitkm.DTO.ViewModels.Subscription;
using Baitkm.Enums;
using Baitkm.Enums.Notifications;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Users.Guests
{
    public interface IGuestService
    {
        Task<GuestProfileModel> GuestProfileAsync(Language language, string deviceId);
        Task<bool> AddGuestAsync(string deviceId, string deviceToken, OsType osType, Language language, string currencyCode);
        Task<int> GetSupportConversationId(Language language, string deviceId);
        Task<bool> EditSubscription(UpdateSubscriptionModel model, string deviceId);
        Task<UpdateSubscriptionModel> GetSubscription(string deviceId);
        Task<PagingResponseModel<PersonNotificationModel>> GuestNotificationList(PagingRequestModel model, string deviceId, Language language);
        bool CheckExistGuest(string deviceId);
    }
}