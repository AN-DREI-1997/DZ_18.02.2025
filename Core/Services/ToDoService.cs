using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Entities;
using static DZ_18._02._2025.Core.Exceptions.MyCustomException;

namespace DZ_18._02._2025.Core.Services
{
    internal class ToDoService : IToDoService
    {
        private readonly List<ToDoItem> _items = new();
        private readonly IToDoRepository _toDoRepository;
        

        public ToDoService(IToDoRepository repository)
        {
            _toDoRepository = repository;
            
        }

        public ToDoItem Add(ToDoUser user, string name, int maxTaskCount, int maxTaskLenght)
        {
            
            if (name.Length > maxTaskLenght)
            {
                throw new TaskLengthLimitException(maxTaskLenght); //Исключение, если превышена длина имени задачи
            }
            if (_toDoRepository.CountActive(user.UserId) >= maxTaskCount)
            {
                throw new TaskCountLimitException(maxTaskCount); //Исключение, если превышено кол-во допустимых задач
            }
            if (_toDoRepository.ExistsByName(user.UserId, name))
            {
                throw new DuplicateTaskException(name); // Исключение, если задача уже существует
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

        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)=>_toDoRepository.Find(user.UserId, t => t.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase));
        
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
