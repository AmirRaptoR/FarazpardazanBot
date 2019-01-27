using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Farazpardazan.ParkingBot.Parking
{
    public class TelegramParkingHandler : ITelegramHandler
    {
        private ParkingLottery _parkingLottery;
        private int[] _adminIds;

        public TelegramParkingHandler(ParkingLottery parkingLottery, IConfiguration configuration)
        {
            _parkingLottery = parkingLottery;
            _adminIds = configuration["telegram:admins"].Split(',').Select(int.Parse).ToArray();
        }

        public bool HandleCallback(ITelegramBotClient client, CallbackQueryEventArgs e)
        {
            if (e.CallbackQuery.Data.StartsWith("PARK_"))
            {
                switch (e.CallbackQuery.Data)
                {
                    case "PARK_PARK_REQ":
                        _parkingLottery.AddVolunteer(e.CallbackQuery.From.Id.ToString(),
                            e.CallbackQuery.From.FirstName + " " + e.CallbackQuery.From.LastName).Wait();
                        client.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id,
                            "شما در قرعه کشی بعدی شرکت خواهید کرد");
                        break;

                    case "PARK_LOTT_REQ":
                        var result = _parkingLottery.GetCurrentParticipatingVolunteers().Result;
                        client.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id,
                            $"شرکت کنندگان در قرعه کشی : {Environment.NewLine} {string.Join(Environment.NewLine, result.Select(x => x.Name))}");
                        break;
                }

                client.DeleteMessageAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId);
                return true;
            }
            return false;
        }

        public bool HandleMessage(ITelegramBotClient client, MessageEventArgs e)
        {
            if (e.Message.Text.Contains("parking", StringComparison.InvariantCultureIgnoreCase))
            {
                switch (e.Message.Text)
                {
                    case "/request_parking":
                        _parkingLottery.AddVolunteer(e.Message.From.Id.ToString(),
                            e.Message.From.FirstName + " " + e.Message.From.LastName).Wait();
                        client.SendTextMessageAsync(e.Message.Chat.Id,
                            "شما در قرعه کشی بعدی شرکت خواهید کرد");
                        break;
                    case "/parking_lottery":
                        var result = _parkingLottery.GetCurrentParticipatingVolunteers().Result;
                        client.SendTextMessageAsync(e.Message.Chat.Id,
                            $"شرکت کنندگان در قرعه کشی : {Environment.NewLine} {string.Join(Environment.NewLine, result.Select(x => x.Name))}");
                        break;
                    case "/parking_lottery_draw":
                        if (!_adminIds.Contains(e.Message.From.Id ))
                        {
                            client.SendTextMessageAsync(e.Message.Chat.Id,"شما رئیس من نیستید. لطفا به رئیس خود بگویید بیاید");
                        }
                        else
                        {
                            if (DateTime.Now.DayOfWeek != DayOfWeek.Friday)
                            {
                                client.SendTextMessageAsync(e.Message.Chat.Id, "قرعه کشی فقط روزای جمعه");
                            }
                            else
                            {
                                var todayLottery = _parkingLottery.GetLotteries().Result
                                    .FirstOrDefault(x => x.Time.Date == DateTime.Today);
                                if (todayLottery!=null)
                                {
                                    client.SendTextMessageAsync(e.Message.Chat.Id, "امروز قرعه کشی یبار انجام شده");
                                    client.SendTextMessageAsync(e.Message.Chat.Id, $"برنده هم [{todayLottery.WinnerName}](tg://user?id={todayLottery.WinnerId}) بودند");
                                }
                                else
                                {
                                    var winner = _parkingLottery.Draw().Result;
                                    client.SendTextMessageAsync(e.Message.Chat.Id, $"تبریک به دوست عزیزمون [{winner.Name}](tg://user?id={winner.Id})\n\r ایشون بار {winner.WonCount}ام هست که برنده می‌شن");
                                }
                            }
                        }
                        break;
                }
                return true;
            }

            return false;
        }

    }
}