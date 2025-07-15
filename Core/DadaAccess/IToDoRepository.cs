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
        IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId);
        /// <summary>
        /// Метод возвращает ToDoItem для UserId со статусом Active.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);
        /// <summary>
        /// Метод Get позволяет получить конкретную задачу по её уникальному идентификатору id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ToDoItem? Get(Guid id);
        /// <summary>
        /// Метод Add добавляет новую задачу в репозиторий. Он принимает объект ToDoItem.
        /// </summary>
        /// <param name="item"></param>
        void Add(ToDoItem item);
        /// <summary>
        /// Метод Update обновляет существующую задачу. Он принимает объект ToDoItem, который должен содержать актуальные данные для обновления.
        /// </summary>
        /// <param name="item"></param>
        void Update(ToDoItem item);
        /// <summary>
        /// Метод Delete удаляет задачу по её идентификатору id. Это позволяет управлять жизненным циклом задач, удаляя те, которые больше не нужны.
        /// </summary>
        /// <param name="id"></param>
        void Delete(Guid id);
        /// <summary>
        /// Метод проверяет есть ли задача с таким именем у пользователя.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        bool ExistsByName(Guid userId, string name);
        /// <summary>
        /// Метод возвращает количество активных задач для указанного пользователя
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        int CountActive(Guid userId);
        /// <summary>
        /// Метод возвращает все задачи пользователя, которые удовлетворяют предикату
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate);
    }
}
