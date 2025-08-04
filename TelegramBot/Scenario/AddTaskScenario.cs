using DZ_18._02._2025.Core.Entities;
using DZ_18._02._2025.Core.Services;
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
    internal class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;

        public AddTaskScenario(IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoService = toDoService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.AddTask;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            ScenarioResult result = default(ScenarioResult);
            ReplyKeyboardMarkup replyMarkup;
            switch (context.CurrentStep)
            {
                case null:
                    replyMarkup = new ReplyKeyboardMarkup(new[]
                    {
                    new KeyboardButton[] { "/cancel",}
                })
                    {
                        ResizeKeyboard = true,
                    };

                    await bot.SendMessage(update.Message.Chat, "Введите название задачи:", cancellationToken: ct, replyMarkup: replyMarkup);

                    context.CurrentStep = "Name";

                    result = ScenarioResult.Transition;

                    break;
                case "Name":
                    string name = update.Message.Text;

                    if (!string.IsNullOrEmpty(name))
                    {
                        context.Data.Add("Name", name);

                        context.CurrentStep = "Deadline";

                        await bot.SendMessage(update.Message.Chat, "Введите дату завершения задачи:", cancellationToken: ct);

                        return ScenarioResult.Transition;
                    }

                    break;
                case "Deadline":
                    string deadline = update.Message.Text;

                    if (!string.IsNullOrEmpty(deadline))
                    {
                        ToDoUser toDoUser = _userService.GetUserAsync(update?.Message?.From?.Id ?? 0, ct).Result;

                        if (!DateTime.TryParse(deadline, out DateTime deadlineDate))
                        {
                            await bot.SendMessage(update.Message.Chat, "Дата ожидается в формате dd.MM.yyyy", cancellationToken: ct);

                            return ScenarioResult.Transition;
                        }

                        _toDoService.AddAsync(toDoUser, (string)context.Data["Name"], deadlineDate, ct);

                        await bot.SendMessage(update.Message.Chat, "Задача добавлена.", cancellationToken: ct, replyMarkup: KeyboardHelper.GetDefaultKeyboard());

                        return ScenarioResult.Completed;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Непредусмотренный к обработке шаг \"{context.CurrentStep}\"");
            }
            return result;
        }
       
    }
}
