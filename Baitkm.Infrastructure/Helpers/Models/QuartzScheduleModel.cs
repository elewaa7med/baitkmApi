using Quartz;
using System.Dynamic;

namespace Baitkm.Infrastructure.Helpers.Models
{
    public class QuartzScheduleModel
    {
        public string Name { get; set; }
        public TriggerBuilder TriggerBuilder { get; set; }
        public ExpandoObject DataMap { get; set; }
        public bool IsListenerRequested { get; set; }
    }
}
