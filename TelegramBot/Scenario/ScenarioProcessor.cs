using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Scenarios;

namespace DZ_18._02._2025.TelegramBot.Scenario
{
    public class ScenarioProcessor
    {
        private readonly IScenarioProvider _scenarioProvider;
        private readonly IScenarioContextRepository _contextRepository;
        private readonly ITelegramBotClient _bot;

        public ScenarioProcessor(
            IScenarioProvider scenarioProvider,
            IScenarioContextRepository contextRepository,
            ITelegramBotClient bot)
        {
            _scenarioProvider = scenarioProvider;
            _contextRepository = contextRepository;
            _bot = bot;
        }

        public async Task ProcessScenario(ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenario = _scenarioProvider.GetScenario(context.CurrentScenario);
            var result = await scenario.HandleMessageAsync(_bot, context, update, ct);

            if (result == ScenarioResult.Completed)
            {
                await _contextRepository.ResetContext(context.UserId, ct);
            }
            else
            {
                await _contextRepository.SetContext(Guid.Parse(context.UserId.ToString()), context, ct);
            }
        }
    }
}
