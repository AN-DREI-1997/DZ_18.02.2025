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
        IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix);
        IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId);

        /// <summary>
        /// Возвращает ToDoItem для UserId со статусом Active
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);
        ToDoItem Add(ToDoUser user, string name, int maxTaskCount, int maxTasklength);
        void MarkCompleted(Guid id);
        void Delete(Guid id);
    }
}
