using Telegram.client;
using Telegram.config;

namespace Telegram;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var client = new TelegramHttpProxyClient(
            TelegramConfig.AppId,
            TelegramConfig.ApiHash,
            TelegramConfig.PhoneNumber,
            Console.ReadLine,
            "127.0.0.1",
            7890
        );
        var user = await client.LoginUserIfNeeded();
        if (user == null) return;

        await client.DownloadMessagesFromUrl(args[0], args[1]);
    }
}