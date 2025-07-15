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
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository) 
        {
            _userRepository = userRepository;
        }

        public ToDoUser? GetUser(long telegramUserId)
        {
            return _userRepository.GetUserByTelegramUserId(telegramUserId);
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
           var existUser = _userRepository.GetUserByTelegramUserId(telegramUserId);
           if (existUser != null)
            {
                  return existUser;
            }
           var newUser = new ToDoUser();
           _userRepository.Add(newUser); // Сохраняем пользователя в репозитории

           return newUser;
        }
    }
}
