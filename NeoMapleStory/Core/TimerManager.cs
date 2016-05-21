using System;
using Quartz;
using Quartz.Impl;

namespace NeoMapleStory.Core
{
    public class TimerManager
    {
        private readonly IScheduler m_scheduler;

        public TimerManager()
        {
            m_scheduler = StdSchedulerFactory.GetDefaultScheduler();
        }

        public static TimerManager Instance { get; } = new TimerManager();

        public bool IsStarted => m_scheduler.IsStarted;

        public void Start() => m_scheduler.Start();

        public void Stop() => m_scheduler.Shutdown();


        public TriggerKey RepeatTask<T>(long repeatTime, long delay = 0) where T : IJob
        {
            var timespan = TimeSpan.FromMilliseconds(repeatTime);
            var job = JobBuilder.Create<T>().Build();
            var trigger =
                TriggerBuilder.Create()
                    .StartAt(DateTime.Now.AddMilliseconds(delay))
                    .WithSimpleSchedule(x => x.WithInterval(timespan).RepeatForever())
                    .Build();

            m_scheduler.ScheduleJob(job, trigger);
            m_scheduler.Start();
            return trigger.Key;
        }

        public TriggerKey RepeatTask(Action task, long repeatTime, long delay = 0)
        {
            var timespan = TimeSpan.FromMilliseconds(repeatTime);

            var jobdata = new JobDataMap {{"Action", task}};

            var job = JobBuilder.Create<ActionToIJob>().UsingJobData(jobdata).Build();

            var trigger =
                TriggerBuilder.Create()
                    .StartAt(DateTime.Now.AddMilliseconds(delay))
                    .WithSimpleSchedule(x => x.WithInterval(timespan).RepeatForever())
                    .Build();

            m_scheduler.ScheduleJob(job, trigger);
            m_scheduler.Start();
            return trigger.Key;
        }

        public TriggerKey RunOnceTask<T>(long delay = 0) where T : IJob
        {
            var job = JobBuilder.Create<T>().Build();
            var trigger =
                TriggerBuilder.Create()
                    .StartAt(DateTime.Now.AddMilliseconds(delay))
                    .Build();

            m_scheduler.ScheduleJob(job, trigger);
            m_scheduler.Start();
            return trigger.Key;
        }

        public TriggerKey RunOnceTask(Action task, long delay = 0)
        {
            var jobdata = new JobDataMap {{"Action", task}};

            var job = JobBuilder.Create<ActionToIJob>().UsingJobData(jobdata).Build();

            var trigger =
                TriggerBuilder.Create()
                    .StartAt(DateTime.Now.AddMilliseconds(delay))
                    .Build();

            m_scheduler.ScheduleJob(job, trigger);
            m_scheduler.Start();
            return trigger.Key;
        }

        public bool CancelTask(TriggerKey triggerKey) => m_scheduler.UnscheduleJob(triggerKey);

        public class ActionToIJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                var action = context.JobDetail.JobDataMap.Get("Action") as Action;
                action?.Invoke();
            }
        }
    }
}