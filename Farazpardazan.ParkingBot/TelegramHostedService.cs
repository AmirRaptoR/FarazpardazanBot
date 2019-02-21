using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Farazpardazan.ParkingBot.Parking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Farazpardazan.ParkingBot
{
    public class TelegramHostedService : IHostedService
    {
        private readonly IEnumerable<ITelegramHandler> _telegramHandlers;
        private readonly ILogger<TelegramHostedService> _logger;
        private readonly ITelegramBotClient _botClient;

        public TelegramHostedService(IEnumerable<ITelegramHandler> telegramHandlers, IConfiguration configuration,
            ILogger<TelegramHostedService> logger)
        {
            _telegramHandlers = telegramHandlers;
            _logger = logger;
            var token = configuration.GetValue<string>("telegram:token");
            _botClient = new TelegramBotClient(token);


                _botClient.OnMessage += BotClientOnOnMessage;
                _botClient.OnCallbackQuery += BotClientOnOnCallbackQuery;
        }

        private void BotClientOnOnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            _logger.LogDebug($"Callback received [from:{e.CallbackQuery.From}] : {e.CallbackQuery.Data}");
        
            foreach (var telegramHandler in _telegramHandlers)
            {
                if (telegramHandler.HandleCallback(_botClient, e))
                {
                    return;
                }
            }
            _logger.LogWarning($"No handler for {e.CallbackQuery.Data}");
        
        }
        
        private void BotClientOnOnMessage(object sender, MessageEventArgs e)
        {
            var type = e.Message.Entities?.FirstOrDefault()?.Type ?? MessageEntityType.Unknown;
            _logger.LogDebug($"Message received [from:{e.Message.From} - chat:{e.Message.Chat.Type}] : {e.Message.Text} ({type})");
            foreach (var telegramHandler in _telegramHandlers)
            {
                if (telegramHandler.HandleMessage(_botClient, e))
                {
                    return;
                }
            }
            _logger.LogWarning($"No handler for {e.Message.Text}");
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Trying to connect to bot");
            _botClient.StartReceiving(cancellationToken: cancellationToken);
            var me = await _botClient.GetMeAsync(cancellationToken);
            _logger.LogInformation($"Successfully connected to {me.FirstName} {me.LastName} [id:{me.Id},user:{me.Username}]");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _botClient.StopReceiving();
            return Task.CompletedTask;
        }
    }
}