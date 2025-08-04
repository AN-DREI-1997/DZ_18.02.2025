using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Scenarios;

namespace DZ_18._02._2025.TelegramBot.Scenario
{
    public interface IScenario
    {
        /// <summary>
        /// Проверяет, может ли текущий сценарий обрабатывать указанный тип сценария.
        /// Используется для определения подходящего обработчика в системе сценариев
        /// </summary>
        /// <param name="scenario"></param>
        /// <returns></returns>
        bool CanHandle(ScenarioType scenario);

        
        /// <summary>
        /// Обрабатывает входящее сообщение от пользователя в рамках текущего сценария.
        ///Включает основную бизнес-логику
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="context"></param>
        /// <param name="update"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct);
    }
}
