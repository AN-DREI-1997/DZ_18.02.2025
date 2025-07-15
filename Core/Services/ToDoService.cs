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
        private readonly IToDoRepository _toDoRepository;
        private int maxTaskLenght;
        private int maxTaskCount;

       
        public ToDoService(IToDoRepository repository, int maxTaskCount, int maxTasklength)
        {
            _toDoRepository = repository;
            maxTaskLenght = maxTaskCount;
            maxTaskCount = maxTasklength;
            
        }
        public ToDoItem Add(ToDoUser user, string name)
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
            var todo = new ToDoItem(user, name);
            _toDoRepository.Add(todo); // Сохраняем задачу в репозитории
            return todo;
        }
        public void Delete(Guid id)
        {
            _toDoRepository.Delete(id);
        }

        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)=> _toDoRepository.Find(user.UserId, t => t.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase));

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
          return  _toDoRepository.GetActiveByUserId(userId);
        }
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid guid)
        { 
          return _toDoRepository.GetAllByUserId(guid);
        }
        public void MarkCompleted(Guid id)
        {
            var todo = _toDoRepository.Get(id);
            if (todo != null)
            {
                todo.State = ToDoItemState.Completed;
                todo.StateChangedAt = DateTime.UtcNow;
                _toDoRepository.Update(todo); // ОБНОВЛЯЕМ задачу в репозитории
            }
        }

    }
}
