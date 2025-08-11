using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DZ_18._02._2025.TelegramBot.Scenario
{
    internal class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        private ConcurrentDictionary <long, ScenarioContext> dict_contexts = new();

        public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            dict_contexts.TryGetValue(userId, out var context);
            return Task.FromResult(context);
        }

        public Task ResetContext(long userId, CancellationToken ct)
        {
            dict_contexts.Remove(userId, out _);
            return Task.CompletedTask;
        }

        public Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            dict_contexts[userId] = context;
            return Task.CompletedTask;
        }
    }
}
