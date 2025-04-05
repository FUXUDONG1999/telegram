using downloader.manager;
using downloader.model;
using Telegram.client;
using Telegram.config;
using TL;
using Chat = Telegram.models.Chat;

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
        var messageGroup = await _client.GetMessage(data.ChatId, data.MessageId);
        var messages = messageGroup.Messages;
        if (messages.Length <= 0) return;

        var mediaMessage = messages.First(m => m is Message { media: MessageMediaDocument });
        if (mediaMessage is Message { media: MessageMediaDocument { document: Document document } })
        {
            var filename = $"{mediaMessage.ID}-{document.Filename}";
            var filePath = $"{outputDir}/{filename}";

            task.FilePath = filePath;
            task.TaskName = filename;
            task.FileSize = document.size;

            var fileStream = File.Create(filePath);
            await _client.DownloadFileAsync(
                document,
                fileStream,
                null,
                (progress, total) => { task.Progress = (double)progress / total; }
            );

            fileStream.Close();
        }
    }
}