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
    internal class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _todos = new();

        public Task AddAsync(ToDoItem item, CancellationToken cancellationToken) =>  Task.Run(() => _todos.Add(item), cancellationToken);

        public async Task<int> CountActiveAsync(Guid userId, CancellationToken cancellationToken)
        {
          return await Task.FromResult(_todos.Count(t => t.User.UserId == userId && t.State == ToDoItemState.Active));
        }

        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_todos.Any(t => t.User.UserId == userId && t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_todos.Where(t => t.User.UserId == userId && predicate(t)).ToList());
        }

        public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken cancellationToken)
        {
          return await Task.FromResult(_todos.FirstOrDefault(g => g.Id == id));
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_todos.Where(t => t.User.UserId == userId).ToList());
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_todos.Where(t => t.User.UserId == userId).ToList());
        }

        public Task UpdateAsync(ToDoItem item, CancellationToken cancellationToken)
        {
            var existingItem = _todos.FirstOrDefault(t => t.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.Name = item.Name;
                existingItem.State = item.State;
                existingItem.StateChangedAt = item.StateChangedAt;
            }

            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var item = _todos.FirstOrDefault(t => t.Id == id);
            if (item != null)
            {
                await Task.Run(() => _todos.Remove(item), cancellationToken);
            }
        }
    }
}
