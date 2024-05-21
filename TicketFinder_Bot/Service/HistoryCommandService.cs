using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TicketFinder_Bot.Helper;
using TicketFinder_Bot.Service.IService;
using TicketFinder_Models;

namespace TicketFinder_Bot.Service
{
    public class HistoryCommandService : IHistoryCommandService
    {
        private readonly IUserHistoryService _userHistoryService;

        public HistoryCommandService(IUserHistoryService userHistoryService)
        {
            _userHistoryService = userHistoryService;
        }

        public async Task HistoryCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            ResponseModelDTO response = await _userHistoryService.GetUserHistory(message.Chat.Id);
            if (response.IsSuccess)
            {
                UserHistoryDTO userHistoryDTO = (UserHistoryDTO)response.Data!;
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Історія останніх 5 пошуків:",
                    replyMarkup: ReplyKeyboards.GetUserHistoryMarkup(userHistoryDTO),
                    disableNotification: true,
                    cancellationToken: cancellationToken);
                return;
            }
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: response.Message!,
                disableNotification: true,
                cancellationToken: cancellationToken);
        }
    }
}
