using Baitkm.BLL.Services.Scheduler.Jobs;
using Baitkm.Infrastructure.Constants;
using Baitkm.Infrastructure.Helpers;
using Baitkm.Infrastructure.Helpers.Models;
using Microsoft.EntityFrameworkCore.Internal;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace Baitkm.Application.ApplicationStartup
{
    public class SchedulerStartup
    {
        public void Start()
        {
            if (ConstValues.Scheduler != null)
                return;
            var schedulerFactory = new StdSchedulerFactory(ConstValues.QuartzConfigs);
            ConstValues.Scheduler = schedulerFactory.GetScheduler().Result;
            ConstValues.Scheduler.Start().Wait();
            var jobs = ConstValues.Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()).Result;
            if (jobs.Any())
                ConstValues.Scheduler.ResumeJobs(GroupMatcher<JobKey>.AnyGroup());
            if (!ConstValues.IsServerConnected)
                return;
            var statisticsJob = new JobKey($"{nameof(StatisticsDataRemoveJob)}#{StatisticsDataRemoveJob.Name}");
            var statisticsJobDetails = ConstValues.Scheduler.GetJobDetail(statisticsJob).Result;
            if (statisticsJobDetails == null)
            {
                //--Not Cron DateTime
                //var trigger = TriggerBuilder.Create()
                //    .WithIdentity($"{nameof(StatisticsDataRemoveJob)}#{StatisticsDataRemoveJob.Name}Trigger")
                //    .StartNow()
                //    .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromDays(1)).RepeatForever());
                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"{nameof(StatisticsDataRemoveJob)}Trigger#{StatisticsDataRemoveJob.Name}")
                    .WithCronSchedule("0 0 0 ? * * *");
                SchedulerHelper.Schedule<StatisticsDataRemoveJob, IJobListener>(new QuartzScheduleModel
                {
                    Name = StatisticsDataRemoveJob.Name,
                    TriggerBuilder = trigger,
                    IsListenerRequested = false,
                    DataMap = null
                }).ConfigureAwait(true);
            }
            var currencyJob = new JobKey($"{nameof(AnnoucmentPricesJob)}#{AnnoucmentPricesJob.Name}");
            var currencyJobDetails = ConstValues.Scheduler.GetJobDetail(currencyJob).Result;
            if (currencyJobDetails == null)
            {
                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"{nameof(AnnoucmentPricesJob)}Trigger#{AnnoucmentPricesJob.Name}")
                    .WithCronSchedule("0 0 10 ? * * *");
                SchedulerHelper.Schedule<AnnoucmentPricesJob, IJobListener>(new QuartzScheduleModel
                {
                    Name = AnnoucmentPricesJob.Name,
                    TriggerBuilder = trigger,
                    IsListenerRequested = false,
                    DataMap = null
                }).ConfigureAwait(true);
            }
        }

        public void Stop()
        {
            if (ConstValues.Scheduler == null)
                return;
            if (ConstValues.Scheduler.Shutdown(waitForJobsToComplete: true).Wait(30000))
                ConstValues.Scheduler = null;
            else
                ConstValues.Scheduler.PauseAll();
            ConstValues.Scheduler = null;
        }
    }
}