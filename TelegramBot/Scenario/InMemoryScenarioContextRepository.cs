using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DZ_18._02._2025.TelegramBot.Scenario
{
    internal class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        private readonly Dictionary<long, ScenarioContext> dict_contexts = new();

        public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            dict_contexts.TryGetValue(userId, out var context);
            return Task.FromResult(context);
        }

        public Task ResetContext(long userId, CancellationToken ct)
        {
            dict_contexts.Remove(userId);
            return Task.CompletedTask;
        }

        public Task SetContext(Guid userId, ScenarioContext context, CancellationToken ct)
        {
            dict_contexts[long.Parse(userId.ToString())] = context;
            return Task.CompletedTask;
        }
    }
}
