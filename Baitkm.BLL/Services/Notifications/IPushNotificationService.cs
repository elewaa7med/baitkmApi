using Baitkm.DTO.ViewModels.Helpers.Paging;
using Baitkm.DTO.ViewModels.PushNotifications;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Notifications
{
    public interface IPushNotificationService
    {
        Task<PagingResponseModel<PushNotificationListModel>> GetList(PagingRequestModel model);
        Task<bool> Create(CreatePushNotificationModel model);
        Task<bool> Delete(int id);
        Task<PushNotificationDetailsModel> Details(int id);
        Task<bool> Update(UpdatePushNotificationModel model);
    }
}