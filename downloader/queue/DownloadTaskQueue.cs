using System.Collections.Concurrent;
using downloader.model;

namespace downloader.queue;

public class DownloadTaskQueue<T>
{
    private readonly BlockingCollection<DownloadTask<T>> _queue = new();

    public void Enqueue(DownloadTask<T> task)
    {
        _queue.Add(task);
    }

    public DownloadTask<T> Dequeue()
    {
        return _queue.Take();
    }
}