using DZ_18._02._2025.Core.Entities;
using DZ_18._02._2025.Core.Services;
using DZ_18._02._2025.TelegramBot.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Scenarios;

namespace DZ_18._02._2025.TelegramBot.Scenario
{
    public class AddTaskScenario : IScenario
    {

        private readonly IUserService _userService;

        private readonly IToDoListService _toDoListService;

        private readonly IToDoService _toDoService;

        public AddTaskScenario(IUserService userService, IToDoListService toDoListService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoListService = toDoListService;
            _toDoService = toDoService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            ReplyKeyboardMarkup replyMarkup;
            ToDoUser toDoUser;
            switch (context.CurrentStep)
            {
                case null:
                    toDoUser = _userService.GetUserAsync(update?.Message?.From?.Id ?? 0, ct).Result;

                    context.Data.Add("User", toDoUser);

                    replyMarkup = new ReplyKeyboardMarkup(new[]
                    {
                    new KeyboardButton[] { "/cancel",}
                })
                    {
                        ResizeKeyboard = true,
                    };

                    context.CurrentStep = "Name";

                    await bot.SendMessage(update.Message.Chat, "Введите название задачи:", cancellationToken: ct, replyMarkup: replyMarkup);

                    return ScenarioResult.Transition;
                case "Name":
                    string name = update.Message.Text;

                    if (!string.IsNullOrEmpty(name))
                    {
                        context.Data.Add("Name", name);

                        context.CurrentStep = "Deadline";

                        await bot.SendMessage(update.Message.Chat, "Введите дату завершения задачи:", cancellationToken: ct);

                    }

                    return ScenarioResult.Transition;

                case "Deadline":
                    string deadline = update.Message.Text;

                    if (!DateTime.TryParse(deadline, out DateTime deadlineDate))
                    {
                        await bot.SendMessage(update.Message.Chat, "Дата ожидается в формате dd.MM.yyyy", cancellationToken: ct);

                        return ScenarioResult.Transition;
                    }
                    context.Data.Add("Deadline", deadlineDate);

                    context.CurrentStep = "List";

                    List<InlineKeyboardButton[]> inlineKeyboardButtonsList = new List<InlineKeyboardButton[]>();

                    inlineKeyboardButtonsList.Add(new[] { InlineKeyboardButton.WithCallbackData("Без списка", "addtask|") });

                    inlineKeyboardButtonsList.AddRange(
                        _toDoListService.GetUserLists(((ToDoUser)context.Data["User"]).UserId, ct).Result.Select(
                            l => new[] { InlineKeyboardButton.WithCallbackData(l.Name, "addtask|" + l.Id) }));

                    var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardButtonsList);

                    await bot.SendMessage(update.Message.Chat.Id,
                        "Выберите список",
                        cancellationToken: ct,
                        replyMarkup: inlineKeyboard);
                    return ScenarioResult.Transition;
                case "List":
                    var toDoListCallbackDto = ToDoListCallbackDto.FromString(update.CallbackQuery.Data);
                    await _toDoService.AddAsync((ToDoUser)context.Data["User"],
                                        (string)context.Data["Name"],
                                        (DateTime)context.Data["Deadline"],
                                        ct,
                                        _toDoListService.Get(toDoListCallbackDto.ToDoListId ?? Guid.Empty, ct).Result);
                    await bot.SendMessage(update.CallbackQuery.Message.Chat, "Задача добавлена.", cancellationToken: ct, replyMarkup: KeyboardHelper.GetDefaultKeyboard());
                    return ScenarioResult.Completed;
                default:
                    throw new ArgumentOutOfRangeException($"Непредусмотренный к обработке шаг \"{context.CurrentStep}\"");
            }
        }
    }
}
