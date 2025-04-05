using System.Collections.Concurrent;
using downloader.model;

namespace downloader.queue;

public class DownloadTaskQueue<T> : IDownloadQueue<T>
{
    private readonly ConcurrentDictionary<string, DownloadTask<T>> _activeTasks = [];

    private readonly ConcurrentDictionary<string, DownloadTask<T>> _completedTasks = [];

    private readonly BlockingCollection<DownloadTask<T>> _queue = new();

    public int ActiveTasksCount => _activeTasks.Count;

    public void Enqueue(DownloadTask<T> task)
    {
        _queue.Add(task);
    }

    public DownloadTask<T> Dequeue()
    {
        return _queue.Take();
    }

    public void AddActive(DownloadTask<T> task)
    {
        _activeTasks[task.TaskId] = task;
    }

    public void AddCompleted(DownloadTask<T> task)
    {
        _activeTasks.Remove(task.TaskId, out _);
        _completedTasks[task.TaskId] = task;
    }

    public IEnumerable<DownloadTask<T>> GetTasks()
    {
        return _activeTasks.Values;
    }

    public DownloadTask<T>? GetTask(string taskId)
    {
        return _activeTasks.GetValueOrDefault(taskId) ?? _completedTasks.GetValueOrDefault(taskId);
    }
}