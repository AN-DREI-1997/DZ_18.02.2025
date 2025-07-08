using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZ_18._02._2025.Entities;

namespace DZ_18._02._2025.Services
{
    public class ToDoService : IToDoService
    {
        private readonly List<ToDoItem> _items = new();

        public ToDoItem Add(ToDoUser user, string name)
        {
            if (name.Length > 100)
            {
                throw new ArgumentException("Имя задачи слишком длинное!");
            }
            var item = new ToDoItem(user, name);
            _items.Add(item);
            return item;
        }
        public void Delete(Guid id)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                _items.Remove(item);
            }
        }
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId) => _items.Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active).ToList();

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid guid) => _items.Where(x => x.User.UserId == guid).ToList();

        public void MarkCompleted(Guid id)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                item.State = ToDoItemState.Completed;
                item.StateChangedAt = DateTime.UtcNow;
            }
        }

    }
}
