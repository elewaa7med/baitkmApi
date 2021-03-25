using Baitkm.BLL.Services.Scheduler.Service;
using Quartz;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Scheduler.Jobs
{

    public class StatisticsDataRemoveJob : IJob
    {
        private readonly ISchedulerService _service = new SchedulerService();
        public static string Name { get; } = "deleteStatistics";

        public async Task Execute(IJobExecutionContext context)
         {
            await _service.DeleteStatistics();
        }
    }
}