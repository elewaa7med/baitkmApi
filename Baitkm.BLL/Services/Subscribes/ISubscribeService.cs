using Baitkm.DTO.ViewModels.Subscribes;
using Baitkm.Enums;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Subscribes
{
    public interface ISubscribeService
    {
        Task<bool> SubscribeAsync(AddSubscribeRequestModel model, int userId, string deviceId, Language language);
        Task<bool> UnSubscribeAsync(int announcementId, int userId, string deviceId, Language language);
    }
}