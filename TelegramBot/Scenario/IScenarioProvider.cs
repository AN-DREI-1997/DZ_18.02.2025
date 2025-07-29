using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Scenarios;

namespace DZ_18._02._2025.TelegramBot.Scenario
{
    public interface IScenarioProvider
    {
        IScenario GetScenario(ScenarioType scenario);
    }
}
