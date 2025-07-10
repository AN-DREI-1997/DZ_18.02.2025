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

        public async Task<ToDoItem> AddAsync(ToDoUser user, string name, int maxTaskCount, int maxTaskLenght, CancellationToken cancellationToken)
        {
            
            if (name.Length > maxTaskLenght)
            {
                throw new TaskLengthLimitException(maxTaskLenght); //Исключение, если превышена длина имени задачи
            }
            if ((await _toDoRepository.CountActiveAsync(user.UserId, cancellationToken)) >= maxTaskCount)
            {
                throw new TaskCountLimitException(maxTaskCount); //Исключение, если превышено кол-во допустимых задач
            }
            if (await _toDoRepository.ExistsByNameAsync(user.UserId, name, cancellationToken))
            {
                throw new DuplicateTaskException(name); // Исключение, если задача уже существует
            }
            var item = new ToDoItem(user, name);
            await _toDoRepository.AddAsync(item, cancellationToken);
            return item;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            await _toDoRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken cancellationToken)
        {
            return  await _toDoRepository.FindAsync(user.UserId, t => t.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase), cancellationToken);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _toDoRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid guid, CancellationToken cancellationToken)
        {
            return await _toDoRepository.GetAllByUserIdAsync(guid, cancellationToken);
        }

        public async Task MarkCompletedAsync(Guid id, CancellationToken cancellationToken)
        {
            var item = await _toDoRepository.GetAsync(id, cancellationToken);
            if (item != null)
            {
                item.State = ToDoItemState.Completed;
                await _toDoRepository.UpdateAsync(item, cancellationToken);
            }
        }
    }
}
