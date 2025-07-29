using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Scenarios;

namespace DZ_18._02._2025.TelegramBot.Scenario
{
    public class ScenarioProvider : IScenarioProvider
    {
        private readonly IEnumerable<IScenario> _scenarios;

        public ScenarioProvider(IEnumerable<IScenario> scenarios)
        {
            _scenarios = scenarios;
        }

        public IScenario GetScenario(ScenarioType scenario)
        {
            var handler = _scenarios.FirstOrDefault(s => s.CanHandle(scenario));
            if (handler == null)
            {
                throw new InvalidOperationException($"No scenario handler found for {scenario}");
            }
            return handler;
        }
    }
}
