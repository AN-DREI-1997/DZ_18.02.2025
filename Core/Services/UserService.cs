using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Entities;

namespace DZ_18._02._2025.Core.Services
{
    internal class UserService : IUserService
    {
        private readonly Dictionary<long, ToDoUser> _users = new();
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository) 
        {
            _userRepository = userRepository;
        }

        public ToDoUser? GetUser(long telegramUserId)
        {
            // Сначала пытаемся получить пользователя из локального словаря
            if (_users.TryGetValue(telegramUserId, out var user))
            {
                return user;
            }

            // Если не нашли, пробуем получить из репозитория
            user = _userRepository.GetUserByTelegramUserId(telegramUserId);
            if (user != null)
            {
                _users[telegramUserId] = user; // Кэшируем пользователя
            }
            return user;
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            if (!_users.ContainsKey(telegramUserId))
            {
                var newUser = new ToDoUser { TelegramUserId = telegramUserId, TelegramUserName = telegramUserName };
                _users[telegramUserId] = newUser;
                _userRepository.Add(newUser); // Сохраняем пользователя в репозитории
            }
            return _users[telegramUserId];
        }
    }
}
