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
            switch (context.CurrentStep)
            {
                case null:
                    return await HandleInitialStep(bot, context, update, ct);
                case "Name":
                    return await HandleNameStep(bot, context, update, ct);
                default:
                    await bot.SendMessage(
                        context.UserId,
                        "Произошла ошибка в обработке сценария. Сценарий будет сброшен.",
                        replyMarkup: KeyboardHelper.CanceldButtons(),
                        cancellationToken: ct);
                    return ScenarioResult.Completed;
            }
        }
        private async Task<ScenarioResult> HandleInitialStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            var user = await _userService.GetUserAsync(context.UserId, ct);
            context.Data["User"] = user;
            context.CurrentStep = "Name";

            await bot.SendMessage(
                context.UserId,
                "Введите название задачи:",
                replyMarkup: new ReplyKeyboardMarkup(new KeyboardButton("/cancel"))
                {
                    ResizeKeyboard = true
                },
                cancellationToken: ct);

            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> HandleNameStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(update.Message?.Text))
            {
                await bot.SendMessage(
                    context.UserId,
                    "Пожалуйста, введите название задачи.",
                    replyMarkup: KeyboardHelper.CanceldButtons(),
                    cancellationToken: ct);
                return ScenarioResult.Transition;
            }

            context.Data["TaskName"] = update.Message.Text;
            context.CurrentStep = "Deadline";

            await bot.SendMessage(
                context.UserId,
                "Введите срок выполнения задачи в формате дд.ММ.гггг (например, 31.12.2023):",
                replyMarkup: KeyboardHelper.CanceldButtons(),
                cancellationToken: ct);

            return ScenarioResult.Transition;
        }
    }
}
