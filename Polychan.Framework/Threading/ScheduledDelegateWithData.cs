namespace Polychan.GUI.Framework.Threading
{
    internal class ScheduledDelegateWithData<T> : ScheduledDelegate
    {
        public new readonly Action<T> Task;

        public T Data;

        public ScheduledDelegateWithData(Action<T> task, T data, double executionTime = 0, double repeatInterval = -1)
            : base(executionTime, repeatInterval)
        {
            Task = task;
            Data = data;
        }

        protected override void InvokeTask() => Task(Data);

        public override string ToString() => $"method \"{Task.Method}\" targeting \"{Task.Target}\" with data \"{Data}\" executing at {ExecutionTime:N0} with repeat {RepeatInterval}";
    }
}
