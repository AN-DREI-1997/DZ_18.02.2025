using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZ_18._02._2025.Core.Entities;

namespace DZ_18._02._2025.Core.DadaAccess
{
    /// <summary>
    /// Интерфейс для работы с задачами
    /// </summary>
    interface IToDoRepository
    {
        /// <summary>
        /// метод возвращает список всех задач (ToDoItem), связанных с конкретным пользователем, идентифицированным по userId.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        /// <summary>
        /// Метод возвращает ToDoItem для UserId со статусом Active.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        /// <summary>
        /// Метод Get позволяет получить конкретную задачу по её уникальному идентификатору id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ToDoItem?> GetAsync(Guid id, CancellationToken cancellationToken);
        /// <summary>
        /// Метод Add добавляет новую задачу в репозиторий. Он принимает объект ToDoItem.
        /// </summary>
        /// <param name="item"></param>
        Task AddAsync(ToDoItem item, CancellationToken cancellationToken);
        /// <summary>
        /// Метод Update обновляет существующую задачу. Он принимает объект ToDoItem, который должен содержать актуальные данные для обновления.
        /// </summary>
        /// <param name="item"></param>
        Task UpdateAsync(ToDoItem item, CancellationToken cancellationToken);
        /// <summary>
        /// Метод Delete удаляет задачу по её идентификатору id. Это позволяет управлять жизненным циклом задач, удаляя те, которые больше не нужны.
        /// </summary>
        /// <param name="id"></param>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        /// <summary>
        /// Метод проверяет есть ли задача с таким именем у пользователя.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Task <bool> ExistsByNameAsync(Guid userId, string name, CancellationToken cancellationToken);
        /// <summary>
        /// Метод возвращает количество активных задач для указанного пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task <int> CountActiveAsync(Guid userId, CancellationToken cancellationToken);
        /// <summary>
        /// Метод возвращает все задачи пользователя, которые удовлетворяют предикату
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken);
    }
}
