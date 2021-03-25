using Baitkm.DTO.ViewModels.Bases;
using System.Threading.Tasks;

namespace Baitkm.DAL.Repository.Firebase
{
    public interface IFirebaseRepository
    {
        Task<bool> SendIndividualNotification<TModel>(TModel model, bool IsGuest) where TModel : IIndividualNotificationBase;
        Task<bool> SendGroupNotification<TModel>(TModel model) where TModel : IGroupNotificationBase;
        Task<bool> SendCampaignNotification<TModel>(TModel model) where TModel : IGroupNotificationBase;
    }
}
