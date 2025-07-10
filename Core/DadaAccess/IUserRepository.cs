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
       Task<ToDoUser?> GetUserAsync(long userId, CancellationToken cancellationToken);
        //Task ToDoUser? GetUserByTelegramUserId(long telegramUserId);
        Task AddAsync(ToDoUser user, CancellationToken cancellationToken);
    }
}
