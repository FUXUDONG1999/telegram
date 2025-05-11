namespace downloader.model;

public class DownloadTask<T>(
    T data
)
{
    private double _progress;

    public string TaskId { get; set; } = "";

    public DateTime AddTime { get; set; }

    public T Data { get; set; } = data;

    public double Progress
    {
        get => Math.Round(_progress, 2);
        set => _progress = value;
    }

    public string ErrorMessage { get; set; } = "";

    public DownloadTaskStatus Status { get; set; }

    public DateTime BeginTime { get; set; }

    public DateTime EndTime { get; set; }

    public string TaskName { get; set; } = "";

    public string FilePath { get; set; } = "";

    public long FileSize { get; set; }
}