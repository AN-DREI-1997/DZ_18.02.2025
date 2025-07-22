using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZ_18._02._2025.Core.Entities;

namespace DZ_18._02._2025.Core.Services
{
    public interface IToDoService
    {
        /// <summary>
        /// Метод возвращает все задачи пользователя, которые начинаются на namePrefix.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="namePrefix"></param>
        /// <returns></returns>
        Task<IReadOnlyList<ToDoItem>> FindAsync(Guid user, string namePrefix, CancellationToken cancellationToken);
        Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        /// <summary>
        /// Возвращает ToDoItem для UserId со статусом Active
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>

        Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<ToDoItem> AddAsync(ToDoUser user, string name,CancellationToken cancellationToken);
        Task MarkCompletedAsync(Guid id, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    }
}
