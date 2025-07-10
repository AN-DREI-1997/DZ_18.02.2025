using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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

        public async Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken cancellationToken)
        {
            // Сначала пытаемся получить пользователя из локального словаря
            if (_users.TryGetValue(telegramUserId, out var user))
            {
                return user;
            }

            // Если не нашли, пробуем получить из репозитория
            user = await _userRepository.GetUserAsync(telegramUserId, cancellationToken);
            if (user != null)
            {
                _users[telegramUserId] = user; // Кэшируем пользователя
            }
            return user;
        }

        public async Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken cancellationToken)
        {
            if (!_users.ContainsKey(telegramUserId))
            {
                var newUser = new ToDoUser { TelegramUserId = telegramUserId, TelegramUserName = telegramUserName };
                _users[telegramUserId] = newUser;
                await _userRepository.AddAsync(newUser, cancellationToken); // Сохраняем пользователя в репозитории
            }
            return _users[telegramUserId];
        }
    }
}
