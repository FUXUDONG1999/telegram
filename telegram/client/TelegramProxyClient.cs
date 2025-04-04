using Starksoft.Net.Proxy;
using TL;
using WTelegram;

namespace Telegram.client;

public delegate TR Acceept<out TR>();

public abstract class TelegramProxyClient : Client
{
    protected TelegramProxyClient(
        string appId,
        string apiHash,
        string phoneNumber,
        Acceept<string?> verficationCodeProvider,
        string proxyAddress,
        int proxyPort,
        string? proxyUsername = null,
        string? proxyPassword = null
    )
        : base(what => what switch
        {
            "api_id" => appId,
            "api_hash" => apiHash,
            "phone_number" => phoneNumber,
            "verification_code" => verficationCodeProvider.Invoke(),
            "session_pathname" => "./sessions",
            _ => null
        })
    {
        TcpHandler = (address, port) =>
        {
            var proxy = CreateProxyClient(proxyAddress, proxyPort, proxyUsername, proxyPassword);
            return Task.FromResult(proxy.CreateConnection(address, port));
        };
    }

    public async Task<Messages_MessagesBase> GetMessages(int chatId, params InputMessage[] messageIds)
    {
        var mc = await this.Channels_GetChannels(new InputChannel(chatId, 0));
        if (!mc.chats.TryGetValue(chatId, out var chat))
            throw new WTException($"Channel {chatId} not found");

        if (chat is not Channel channel) throw new WTException("URL does not identify a valid Channel");

        return await this.Channels_GetMessages(channel, messageIds);
    }

    public async Task DownloadMessages(string outputDir, int chatId, params InputMessage[] messageIds)
    {
        var messages = await GetMessages(chatId, messageIds);

        var tasks = new List<Task<string>>();
        foreach (var msg in messages.Messages)
            if (msg is Message { media: MessageMediaDocument { document: Document document } })
            {
                var filename = $"{msg.ID}-{document.Filename}";
                tasks.Add(DownloadFileAsync(document, File.Create($"{outputDir}/{filename}")));
            }

        Task.WaitAll(tasks);
    }

    public async Task DownloadMessagesFromUrl(string beginUrl, string endUrl)
    {
        var beginId = getMessageIdFromUrl(beginUrl);
        var endId = getMessageIdFromUrl(endUrl);

        var messageCount = endId - beginId + 1;

        var messageIds = new InputMessage[messageCount];
        for (var i = 0; i < messageCount; i++) messageIds[i] = beginId + i;

        await DownloadMessages("E:\\videos", getChatFromUrl(beginUrl), messageIds);
    }

    protected abstract IProxyClient CreateProxyClient(string proxyAddress, int proxyPort, string? proxyUsername = null, string? proxyPassword = null);

    private int getChatFromUrl(string url)
    {
        var splits = url.Split("/");
        return int.Parse(splits[4]);
    }

    private int getMessageIdFromUrl(string url)
    {
        var splits = url.Split("/");
        return int.Parse(splits[5]);
    }
}

public class TelegramHttpProxyClient(
    string appId,
    string apiHash,
    string phoneNumber,
    Acceept<string?> verificationCodeProvider,
    string proxyAddress,
    int proxyPort,
    string? proxyUsername = null,
    string? proxyPassword = null
)
    : TelegramProxyClient(appId, apiHash, phoneNumber, verificationCodeProvider, proxyAddress, proxyPort, proxyUsername, proxyPassword)
{
    protected override IProxyClient CreateProxyClient(string proxyAddress, int proxyPort, string? proxyUsername = null, string? proxyPassword = null)
    {
        return new HttpProxyClient(proxyAddress, proxyPort);
    }
}

public class TelegramSocks5ProxyClient(
    string appId,
    string apiHash,
    string phoneNumber,
    Acceept<string?> verificationCodeProvider,
    string proxyAddress,
    int proxyPort,
    string? proxyUsername = null,
    string? proxyPassword = null
)
    : TelegramProxyClient(appId, apiHash, phoneNumber, verificationCodeProvider, proxyAddress, proxyPort, proxyUsername, proxyPassword)
{
    protected override IProxyClient CreateProxyClient(string proxyAddress, int proxyPort, string? proxyUsername = null, string? proxyPassword = null)
    {
        return new Socks5ProxyClient(proxyAddress, proxyPort, proxyUsername, proxyPassword);
    }
}