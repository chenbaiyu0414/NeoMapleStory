using System;
using FluentScheduler;


namespace NeoMapleStory.Core.TimeManager
{
     public class TimerManager:Registry
    {
        public static TimerManager Instance { get; private set; }

        public TimerManager()
        {
            Instance = this;
        }

        public void Start() => JobManager.Start();

        public void Stop() => JobManager.Stop();

        /// <summary>
        /// 注册一个任务并按指定的时间重复执行
        /// </summary>
        /// <typeparam name="T">任务</typeparam>
        /// <param name="repeatTime">重复时间，以秒为单位</param>
        /// <param name="delay">推迟时间，以秒为单位</param>
        /// <returns></returns>
        public string RegisterJob<T>(int repeatTime, int delay = 0) where T : IJob
        {
            string name = nameof(T);
            JobManager.AddJob<T>(s => s.WithName(name).ToRunNow().AndEvery(repeatTime).Seconds().DelayFor(delay).Seconds());
            //Schedule<T>().WithName(name).ToRunNow().AndEvery(repeatTime).Seconds().DelayFor(delay).Seconds();
            return name;
        }

        public string RegisterJob(Action action, int repeatTime, int delay = 0)
        {
            string name = nameof(action);
            JobManager.AddJob(action, s => s.WithName(name).ToRunNow().AndEvery(repeatTime).Seconds().DelayFor(delay).Seconds());
            //Schedule(action).WithName(name).ToRunNow().AndEvery(repeatTime).Seconds().DelayFor(delay).Seconds();
            return name;
        }


        /// <summary>
        /// 开始一个任务并立即执行，只执行一次
        /// </summary>
        /// <typeparam name="T">任务</typeparam>
        /// <param name="delay">推迟时间，以秒为单位</param>
        /// <returns></returns>
        public string ScheduleJob<T>(int delay) where T : IJob
        {
            string name = nameof(T);
            JobManager.AddJob<T>(s => s.WithName(name).ToRunOnceIn(delay).Seconds());
            //Schedule<T>().WithName(name).ToRunOnceIn(delay).Seconds();
            return name;
        }

        public string ScheduleJob(Action action, int delay)
        {
            string name = nameof(action);
            JobManager.AddJob(action, s => s.WithName(name).ToRunOnceIn(delay).Seconds());
            //Schedule(action).ToRunOnceIn(delay).Seconds();
            return name;
        }

        public string ScheduleJobAtTimeStamp(Action action, long timestamp)
        {
            string name = nameof(action);
            JobManager.AddJob(action,
                s => s.WithName(name).ToRunOnceIn((int) (timestamp - DateTime.Now.GetTimeMilliseconds())/1000).Seconds());
            //Schedule(action).ToRunOnceIn((int) (timestamp - DateTime.Now.GetTimeMilliseconds())/1000).Seconds();
            return name;
        }

        public void CancelJob(string name) => JobManager.RemoveJob(name);

        ///// <summary>
        ///// 开始一个任务并立即执行，只执行一次
        ///// </summary>
        ///// <typeparam name="T">任务</typeparam>
        ///// <param name="timestamp">Unix时间戳</param>
        ///// <returns></returns>
        //public void ScheduleJob<T>(long timestamp) where T : IJob
        //{
        //   Schedule<T>().ToRunOnceAt(time)
        //}
    }
}
