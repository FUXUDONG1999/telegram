namespace downloader.model;

public class DownloadTask<T>(
    T data
)
{
    public string TaskId { get; set; } = "";

    public DateTime AddTime { get; set; }

    public T Data { get; set; } = data;

    public float Progress { get; set; } = 0.0f;

    public string ErrorMessage { get; set; } = "";

    public DownloadTaskStatus Status { get; set; }


    public DateTime BeginTime { get; set; }

    public DateTime EndTime { get; set; }
}