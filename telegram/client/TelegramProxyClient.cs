using Starksoft.Net.Proxy;
using TL;
using WTelegram;

namespace Telegram.client;

public delegate TR Acceept<out TR>();

public class TelegramProxyClient : Client
{
    public TelegramProxyClient(
        string appId,
        string apiHash,
        string phoneNumber,
        Acceept<string?> verficationCodeProvider,
        string proxyAddress,
        int proxyPort
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
        Helpers.Log = (i, s) => { };
        TcpHandler = (address, port) =>
        {
            var proxy = new HttpProxyClient(proxyAddress, proxyPort);
            return Task.FromResult(proxy.CreateConnection(address, port));
        };
    }

    public async Task<Messages_MessagesBase> GetMessage(int chatId, InputMessage messageId)
    {
        var mc = await this.Channels_GetChannels(new InputChannel(chatId, 0));
        if (!mc.chats.TryGetValue(chatId, out var chat))
            throw new WTException($"Channel {chatId} not found");

        if (chat is not Channel channel) throw new WTException("URL does not identify a valid Channel");

        return await this.Channels_GetMessages(channel, messageId);
    }
}