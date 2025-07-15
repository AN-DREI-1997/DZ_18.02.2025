using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Entities;

namespace DZ_18._02._2025.Infastructure.DataAccess
{
    internal class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _users = new();

        public void Add(ToDoUser user)
        {
            if (!_users.Any(u => u.TelegramUserId == user.TelegramUserId))
            {
                _users.Add(user);
            }
        }

        public ToDoUser? GetUser(Guid userId)
        {
           return  return _users.FirstOrDefault(u => u.UserId == userId);
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
        {
            return _users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);
        }
    }
}
