using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class JobQueue
{
    private bool isFlushing = false;
    private bool isFlushPending = false;
    private readonly Queue<Action> queue = new Queue<Action>();
    private readonly TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

    public JobQueue()
    {
        tcs.SetResult(true);
    }

    public Task NextTick(Action job)
    {
        return tcs.Task.ContinueWith(_ => job());
    }

    public void QueueJob(Action job)
    {
        if (!queue.Contains(job))
        {
            queue.Enqueue(job);
            QueueFlush();
        }
    }

    private void QueueFlush()
    {
        if (!isFlushPending && !isFlushing)
        {
            isFlushPending = true;
            NextTick(FlushJobs);
        }
    }

    private void FlushJobs()
    {
        isFlushing = true;
        isFlushPending = false;
        while (queue.Any())
        {
            var job = queue.Dequeue();
            job();
        }
        isFlushing = false;
    }
}
