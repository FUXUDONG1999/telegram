using downloader.model;

namespace downloader.queue;

public interface IDownloadQueue<T>
{
    public int ActiveTasksCount { get; }

    public void Enqueue(DownloadTask<T> task);

    public DownloadTask<T> Dequeue();

    public void AddActive(DownloadTask<T> task);

    public void AddCompleted(DownloadTask<T> task);

    public IEnumerable<DownloadTask<T>> GetTasks();
}