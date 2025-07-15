using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZ_18._02._2025.Core.Entities;

namespace DZ_18._02._2025.Core.DadaAccess
{
    /// <summary>
    /// Интерфейс для работы с пользователями
    /// </summary>
    interface IUserRepository
    {
        ToDoUser? GetUser(Guid userId);
        ToDoUser? GetUserByTelegramUserId(long telegramUserId);
        void Add(ToDoUser user);
    }
}
