using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Entities;
using static DZ_18._02._2025.Core.Exceptions.MyCustomException;

namespace DZ_18._02._2025.Core.Services
{
    internal class ToDoService : IToDoService
    {
        private readonly FileToDoRepository _toDoRepository;
        private int _maxTaskLenght;
        private int _maxTaskCount;

        public ToDoService(FileToDoRepository repository, int maxTaskCount, int maxTasklength)
        {
            _toDoRepository = repository;
            _maxTaskCount = maxTaskCount;
            _maxTaskLenght = maxTasklength;

        }

        public async Task<ToDoItem> AddAsync(ToDoUser user, string name, DateTime deadline, CancellationToken cancellationToken)
        {

            if (name.Length > _maxTaskLenght)
            {
                throw new TaskLengthLimitException(_maxTaskLenght); //Исключение, если превышена длина имени задачи
            }
            if ((await _toDoRepository.CountActiveAsync(user.UserId, cancellationToken)) >= _maxTaskCount)
            {
                throw new TaskCountLimitException(_maxTaskCount); //Исключение, если превышено кол-во допустимых задач
            }
            if (await _toDoRepository.ExistsByNameAsync(user.UserId, name, cancellationToken))
            {
                throw new DuplicateTaskException(name); // Исключение, если задача уже существует
            }

            var item = new ToDoItem(user, name, deadline);
            await _toDoRepository.AddAsync(item, cancellationToken);
            return item;

        }
        
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {

            await _toDoRepository.DeleteAsync(id, cancellationToken);
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

                _toDoRepository.DeleteAsync(id, cancellationToken);
            }
        }

        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, string namePrefix, CancellationToken cancellationToken)
        {
            return await _toDoRepository.FindAsync(userId, t => t.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase), cancellationToken: cancellationToken);
        }
    }
}
