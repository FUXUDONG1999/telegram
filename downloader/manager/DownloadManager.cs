using System.Collections.Concurrent;
using downloader.model;
using downloader.queue;

namespace downloader.manager;

public abstract class DownloadManager<T>
{
    private readonly ConcurrentDictionary<string, DownloadTask<T>> _activeTasks = [];

    private readonly ConcurrentDictionary<string, DownloadTask<T>> _completedTasks = [];

    private readonly DownloadTaskQueue<T> _downloadTaskQueue = new();

    private volatile bool _isRunning;

    private volatile int _maxWorkers;

    protected DownloadManager(int minWorkers = 8, int maxWorkers = 32)
    {
        ThreadPool.SetMinThreads(minWorkers, minWorkers);
        ThreadPool.SetMaxThreads(maxWorkers, maxWorkers);

        _maxWorkers = maxWorkers;
    }

    public virtual void AddTask(DownloadTask<T> task)
    {
        task.Status = DownloadTaskStatus.Waiting;
        task.AddTime = DateTime.Now;
        task.TaskId = Guid.NewGuid().ToString();

        _downloadTaskQueue.Enqueue(task);
    }

    public virtual void Start()
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            if (_isRunning) throw new InvalidOperationException("Manager is already running");
            _isRunning = true;

            while (_isRunning)
            {
                if (_activeTasks.Count >= _maxWorkers) continue;

                var task = _downloadTaskQueue.Dequeue();
                task.Status = DownloadTaskStatus.Downloading;
                task.BeginTime = DateTime.Now;
                _activeTasks[task.TaskId] = task;

                ThreadPool.QueueUserWorkItem(async void (_) =>
                    {
                        try
                        {
                            if (task.Status is DownloadTaskStatus.Success or DownloadTaskStatus.Error) return;
                            await ProcessTask(task);
                            task.Status = DownloadTaskStatus.Success;
                        }
                        catch (Exception e)
                        {
                            task.ErrorMessage = e.Message;
                        }
                        finally
                        {
                            task.EndTime = DateTime.Now;
                            _activeTasks.Remove(task.TaskId, out var _);
                            _completedTasks[task.TaskId] = task;
                        }
                    },
                    task
                );
            }
        });
    }

    public virtual void Stop()
    {
        if (_isRunning) return;

        _isRunning = false;
    }

    public virtual DownloadTask<T>? GetTask(string taskId)
    {
        var task = _activeTasks.GetValueOrDefault(taskId);
        return task ?? _completedTasks.GetValueOrDefault(taskId);
    }

    protected abstract Task ProcessTask(DownloadTask<T> task);
}