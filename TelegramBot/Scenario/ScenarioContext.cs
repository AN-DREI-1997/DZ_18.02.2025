using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Scenarios;

namespace DZ_18._02._2025.TelegramBot.Scenario
{   
    public class ScenarioContext
    {
        public long UserId { get; set; } // Id пользователя в Telegram
        public ScenarioType CurrentScenario { get; set; }
        public string? CurrentStep { get; set; } // Текущий шаг сценария
        public Dictionary<string, object> Data { get; set; } = new();

        public ScenarioContext(long userId, ScenarioType scenario)
        {
            UserId = userId;
            CurrentScenario = scenario;
        }
    }
}
