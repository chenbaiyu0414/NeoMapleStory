using System;
using Quartz;
using Quartz.Impl;
using Quartz.Util;
using static Quartz.MisfireInstruction;

namespace NeoMapleStory.Core
{
    public class TimerManager
    {
        public static TimerManager Instance { get; } = new TimerManager();
        private readonly IScheduler _scheduler;

        public TimerManager()
        {
            _scheduler = StdSchedulerFactory.GetDefaultScheduler();
        }

        public void Start() => _scheduler.Start();

        public void Stop() => _scheduler.Shutdown();


        public TriggerKey RepeatTask<T>(long repeatTime, long delay = 0) where T : IJob
        {
            TimeSpan timespan = TimeSpan.FromMilliseconds(repeatTime);
            IJobDetail job = JobBuilder.Create<T>().Build();
            ITrigger trigger =
                TriggerBuilder.Create()
                    .StartAt(DateTime.Now.AddMilliseconds(delay))
                    .WithSimpleSchedule(x => x.WithInterval(timespan).RepeatForever())
                    .Build();

            _scheduler.ScheduleJob(job, trigger);
            _scheduler.Start();
            return trigger.Key;
        }

        public TriggerKey RepeatTask(Action task, long repeatTime, long delay = 0)
        {
            TimeSpan timespan = TimeSpan.FromMilliseconds(repeatTime);

            JobDataMap jobdata = new JobDataMap { { "Action", task } };

            IJobDetail job = JobBuilder.Create<ActionToIJob>().UsingJobData(jobdata).Build();

            ITrigger trigger =
                TriggerBuilder.Create()
                    .StartAt(DateTime.Now.AddMilliseconds(delay))
                    .WithSimpleSchedule(x => x.WithInterval(timespan).RepeatForever())
                    .Build();

            _scheduler.ScheduleJob(job, trigger);
            _scheduler.Start();
            return trigger.Key;
        }

        public TriggerKey RunOnceTask<T>(long delay = 0) where T : IJob
        {
            IJobDetail job = JobBuilder.Create<T>().Build();
            ITrigger trigger =
                TriggerBuilder.Create()
                    .StartAt(DateTime.Now.AddMilliseconds(delay))
                    .Build();

            _scheduler.ScheduleJob(job, trigger);
            _scheduler.Start();
            return trigger.Key;
        }

        public TriggerKey RunOnceTask(Action task, long delay = 0)
        {
            JobDataMap jobdata = new JobDataMap { { "Action", task } };

            IJobDetail job = JobBuilder.Create<ActionToIJob>().UsingJobData(jobdata).Build();

            ITrigger trigger =
                TriggerBuilder.Create()
                    .StartAt(DateTime.Now.AddMilliseconds(delay))
                    .Build();

            _scheduler.ScheduleJob(job, trigger);
            _scheduler.Start();
            return trigger.Key;
        }

        public bool CancelTask(TriggerKey triggerKey) => _scheduler.UnscheduleJob(triggerKey);

        public class ActionToIJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                Action action = context.JobDetail.JobDataMap.Get("Action") as Action;
                action?.Invoke();
            }
        }
    }
}
