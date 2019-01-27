using Telegram.Bot;
using Telegram.Bot.Args;

namespace Farazpardazan.ParkingBot
{
    public interface ITelegramHandler
    {
        bool HandleCallback(ITelegramBotClient client, CallbackQueryEventArgs e);
        bool HandleMessage(ITelegramBotClient client, MessageEventArgs e);
    }
}