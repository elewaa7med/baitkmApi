using Baitkm.BLL.Services.Scheduler.Service;
using Quartz;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Scheduler.Jobs
{
    public class AnnouncementDeactivationDayJob : IJob
    {
        private readonly ISchedulerService _service = new SchedulerService();
        public static string Name { get; } = "deactivate";

        public async Task Execute(IJobExecutionContext context)
        {
            await _service.Deactivate();
        }
    }
}
