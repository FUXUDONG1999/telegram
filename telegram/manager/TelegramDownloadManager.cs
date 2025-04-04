using downloader.manager;
using downloader.model;
using Telegram.client;
using Telegram.config;
using Telegram.models;

namespace Telegram.manager;

public class TelegramDownloadManager(string outputDir, Acceept<string?> verficationCodeProvider) : DownloadManager<Chat>
{
    private readonly TelegramProxyClient _client = new(
        TelegramConfig.AppId,
        TelegramConfig.ApiHash,
        TelegramConfig.PhoneNumber,
        verficationCodeProvider,
        "127.0.0.1",
        7890
    );

    public override void Start()
    {
        var userTask = _client.LoginUserIfNeeded();
        if (userTask.Result == null) throw new InvalidOperationException("User is not logged in");

        base.Start();
    }

    protected override async Task ProcessTask(DownloadTask<Chat> task)
    {
        var data = task.Data;
        await _client.DownloadMessageVideos(outputDir, data.ChatId, data.MessageId);
    }
}