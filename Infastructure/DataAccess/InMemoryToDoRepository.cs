using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Entities;

namespace DZ_18._02._2025.Infastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _todos = new();

        public void Add(ToDoItem item)
        {
            _todos.Add(item);
        }

        public int CountActive(Guid userId) => _todos.Count(t => t.User.UserId == userId && t.State == ToDoItemState.Active);

        public void Delete(Guid id)
        {
            var item = _todos.FirstOrDefault(d => d.Id == id);
            if (item != null)
            {
                _todos.Remove(item);
            }
        }

        public bool ExistsByName(Guid userId, string name) => _todos.Any(t => t.User.UserId == userId && t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate) => [.. _todos.Where(t => t.User.UserId == userId && predicate(t))];


        public ToDoItem? Get(Guid id) => _todos.FirstOrDefault(g => g.Id == id);        

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId) => [.. _todos.Where(t => t.User.UserId == userId)];

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId) => [.._todos.Where(t => t.User.UserId == userId)];

        public void Update(ToDoItem item)
        {
            var existingItem = _todos.FirstOrDefault(t => t.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.Name = item.Name;
                existingItem.State = item.State;
                existingItem.StateChangedAt = item.StateChangedAt;
            }
        }
    }
}
