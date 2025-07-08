using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZ_18._02._2025.Entities;

namespace DZ_18._02._2025.Services
{
    internal class UserService : IUserService
    {
        private readonly Dictionary<long, ToDoUser> _users = new();

        public ToDoUser? GetUser(long telegramUserId)
        {
            _users.TryGetValue(telegramUserId, out var user);
            return user;
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            if (!_users.ContainsKey(telegramUserId))
            {
                _users[telegramUserId] = new ToDoUser { TelegramUserId = telegramUserId, TelegramUserName = telegramUserName };
            }
            return _users[telegramUserId];
        }
    }
}
