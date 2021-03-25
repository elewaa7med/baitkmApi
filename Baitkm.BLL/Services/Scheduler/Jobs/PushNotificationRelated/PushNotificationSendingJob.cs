using Baitkm.BLL.Services.Scheduler.Service;
using Quartz;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Scheduler.Jobs.PushNotificationRelated
{
    public class PushNotificationSendingJob : IJob
    {
        private readonly ISchedulerService _service = new SchedulerService();
        public static string Name { get; } = "sendPushNotification";

        public async Task Execute(IJobExecutionContext context)
        {
            var id = context.JobDetail.JobDataMap?.Get("promoId")?.ToString();
            if (string.IsNullOrEmpty(id))
                return;
            int.TryParse(id, out var pushNotificationId);
            await _service.SendPushNotification(pushNotificationId);
        }
    }
}
