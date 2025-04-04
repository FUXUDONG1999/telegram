using System.ComponentModel.DataAnnotations;

namespace telegram_downloader.model;

public class AddRangeTaskRequest
{
    [Required] public string StartUrl { get; set; }

    [Required] public string EndUrl { get; set; }

    public int ChatId => int.Parse(StartUrl.Split("/")[4]);

    public int BeginMessageId => int.Parse(StartUrl.Split("/")[5]);

    public int EndMessageId => int.Parse(EndUrl.Split("/")[5]);
}