using Baitkm.BLL.Services.Scheduler.Service;
using Quartz;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Scheduler.Jobs
{
    public class AnnouncementTopDeactivationDayJob : IJob
    {
        private readonly ISchedulerService _service = new SchedulerService();
        public static string Name { get; } = "removeFromTop";

        public async Task Execute(IJobExecutionContext context)
        {
            await _service.RemoveFromTop();
        }
    }
}
