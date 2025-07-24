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
        private readonly FileUserRepository _userRepository;
        public UserService(FileUserRepository userRepository) 
        {
             _userRepository = userRepository;
        }

        public async Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken cancellationToken)
        {

           return await _userRepository.GetUserByTelegramUserId(telegramUserId);

        }

        public async Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken cancellationToken)
        {

            var user = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, cancellationToken);
            return user; // Возврат объекта

        }
    }
}
