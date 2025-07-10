using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Entities;

namespace DZ_18._02._2025.Infastructure.DataAccess
{
    internal class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _users = new();

        public async Task AddAsync(ToDoUser user, CancellationToken cancellationToken)
        {
            await Task.Run(() => _users.Add(user), cancellationToken);
        }

        public async Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => u.TelegramUserId == telegramUserId));
        }
    }
}
