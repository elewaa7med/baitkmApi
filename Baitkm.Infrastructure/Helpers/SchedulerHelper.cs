using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers.Models;
using Baitkm.Infrastructure.Validation;
using Quartz;
using Quartz.Impl.Matchers;
using System;
using System.Threading.Tasks;

namespace Baitkm.Infrastructure.Helpers
{
    public class SchedulerHelper
    {
        public static async Task Schedule<TJob, TListener>(QuartzScheduleModel model) where TJob : class, IJob where TListener : class, IJobListener
        {
            if (ConstValues.Scheduler == null)
                throw new SmartException("Scheduler instance is disposed");
            var job = JobBuilder.Create<TJob>()
                .WithIdentity($"{typeof(TJob).Name}#{model.Name}")
                .Build();
            if (model.DataMap != null)
                foreach (var dataMap in model.DataMap)
                {
                    job.JobDataMap[dataMap.Key] = dataMap.Value;
                }
            var trigger = model.TriggerBuilder.Build();
            if (model.IsListenerRequested)
                ConstValues.Scheduler.ListenerManager.AddJobListener(Activator.CreateInstance<TListener>(), GroupMatcher<JobKey>.AnyGroup());
            await ConstValues.Scheduler.ScheduleJob(job, trigger);
        }

        public static async Task UnSchedule<T>(string name) where T : class, IJob
        {
            var triggerKey = new TriggerKey($"{typeof(T).Name}Trigger#{name}");
            var jobKey = new JobKey($"{typeof(T).Name}#{name}");
            if (await ConstValues.Scheduler.CheckExists(jobKey))
            {
                await ConstValues.Scheduler.UnscheduleJob(triggerKey);
                await ConstValues.Scheduler.DeleteJob(jobKey);
            }
        }

        public static async Task ReSchedule<TJob, TListener>(QuartzScheduleModel model) where TJob : class, IJob where TListener : class, IJobListener
        {
            if (ConstValues.Scheduler == null)
                throw new SmartException("Scheduler instance is disposed");
            var triggerKey = new TriggerKey($"{typeof(TJob).Name}Trigger#{model.Name}");
            var jobKey = new JobKey($"{typeof(TJob).Name}#{model.Name}");
            var job = await ConstValues.Scheduler.GetJobDetail(jobKey);
            if (job == null)
            {
                var jobToCreate = JobBuilder.Create<TJob>()
                    .WithIdentity($"{typeof(TJob).Name}#{model.Name}")
                    .Build();
                if (model.DataMap != null)
                    foreach (var dataMap in model.DataMap)
                    {
                        jobToCreate.JobDataMap[dataMap.Key] = dataMap.Value;
                    }
                var triggerToCreate = model.TriggerBuilder.Build();
                if (model.IsListenerRequested)
                    ConstValues.Scheduler.ListenerManager.AddJobListener(Activator.CreateInstance<TListener>(), GroupMatcher<JobKey>.AnyGroup());
                await ConstValues.Scheduler.ScheduleJob(jobToCreate, triggerToCreate);
            }
            else
            {
                if (model.DataMap != null)
                    foreach (var dataMap in model.DataMap)
                    {
                        job.JobDataMap[dataMap.Key] = dataMap.Value;
                    }
                var trigger = model.TriggerBuilder.Build();
                if (model.IsListenerRequested)
                    ConstValues.Scheduler.ListenerManager.AddJobListener(Activator.CreateInstance<TListener>(), GroupMatcher<JobKey>.AnyGroup());
                await ConstValues.Scheduler.RescheduleJob(triggerKey, trigger);
            }
        }
    }
}
