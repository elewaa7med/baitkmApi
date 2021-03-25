using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Scheduler.Service
{
    public interface ISchedulerService
    {
        Task RemoveFromTop();
        Task Deactivate();
        Task SendPushNotification(int id);
        Task UnBlock();
        Task DeleteStatistics();
        Task Expired();
        Task CalculatePrices();
    }
}
