namespace NeoMapleStory.Core.TimeManager
{
    public interface ITimerManager
    {
        bool IsTerminated();

        bool IsShutdown();

        long GetCompletedTaskCount();

        long GetActiveCount();

        long GetTaskCount();

        int GetQueuedTasks();
    }
}
