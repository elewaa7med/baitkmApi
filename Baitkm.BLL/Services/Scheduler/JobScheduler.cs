using Baitkm.BLL.Services.Scheduler.Jobs;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;

namespace Baitkm.BLL.Services.Scheduler
{
    public class JobScheduler
    {
        public static async Task Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
            await scheduler.Start();
            IJobDetail job = JobBuilder.Create<AnnouncementTopDeactivationDayJob>().Build();
            ITrigger trigger = TriggerBuilder.Create()
                 .StartNow()
                .WithSimpleSchedule(x => x
                .WithInterval(TimeSpan.FromDays(1))
                .RepeatForever())
                .Build();
            //.WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(12, 00))
            //.WithSimpleSchedule(x => x
            //.WithIntervalInHours(2) 
            //.RepeatForever())

            IScheduler scheduler1 = StdSchedulerFactory.GetDefaultScheduler().Result;
            await scheduler1.Start();
            IJobDetail job1 = JobBuilder.Create<AnnouncementDeactivationDayJob>().Build();
            ITrigger trigger1 = TriggerBuilder.Create()
                .StartNow()
                .WithSimpleSchedule(x => x
                .WithInterval(TimeSpan.FromDays(1))
                .RepeatForever())
                .Build();
            //.WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(12, 00))
            //.WithSimpleSchedule(x => x
            //.WithIntervalInHours(2)
            //.RepeatForever())

            IScheduler scheduler2 = StdSchedulerFactory.GetDefaultScheduler().Result;
            await scheduler2.Start();
            IJobDetail job2 = JobBuilder.Create<StatisticsDataRemoveJob>().Build();
            ITrigger trigger2 = TriggerBuilder.Create()
                .StartNow()
                .WithSimpleSchedule(x => x
                .WithInterval(TimeSpan.FromMinutes(1))
                //.WithInterval(TimeSpan.FromDays(1))
                .RepeatForever())
                .Build();
            //.WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(12, 00))
            //.WithSimpleSchedule(x => x
            //.WithIntervalInHours(2)
            //.RepeatForever())

            await scheduler.ScheduleJob(job, trigger);
            await scheduler1.ScheduleJob(job1, trigger1);
            await scheduler2.ScheduleJob(job2, trigger2);
            await scheduler.Start();
        }
    }
}
