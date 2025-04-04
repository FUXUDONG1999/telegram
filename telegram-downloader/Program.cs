using downloader.model;
using Microsoft.AspNetCore.Mvc;
using telegram_downloader.model;
using Telegram.config;
using Telegram.manager;
using Chat = Telegram.models.Chat;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseHttpsRedirection();

var manager = new TelegramDownloadManager(TelegramConfig.DownloadOutputDir, Console.ReadLine);
manager.Start();

app.MapGet(
    "/addTask/{chatId:int}/{messageId:int}",
    (int chatId, int messageId) =>
    {
        manager.AddTask(new DownloadTask<Chat>(new Chat(chatId, messageId)));

        return "Task added";
    }
);

app.MapPost(
    "/addRangeTask",
    ([FromBody] AddRangeTaskRequest request) =>
    {
        foreach (var i in Enumerable.Range(request.BeginMessageId, request.EndMessageId - request.BeginMessageId + 1))
            manager.AddTask(new DownloadTask<Chat>(new Chat(request.ChatId, i)));

        return "Task added";
    });

app.Run();