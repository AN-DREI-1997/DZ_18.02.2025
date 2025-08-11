using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DZ_18._02._2025.Core.Services
{
    /// <summary>
    /// Сервис для работы со списками дел
    /// </summary>
    public class ToDoListService:IToDoListService
    {
        private readonly IToDoListRepository _toDoListRepository;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса
        /// </summary>
        /// <param name="toDoListRepository">Репозиторий для работы с хранилищем списков</param>
        public ToDoListService(IToDoListRepository toDoListRepository)
        {
            _toDoListRepository = toDoListRepository;
        }

        /// <summary>
        /// Создает новый список дел
        /// </summary>
        /// <param name="user">Пользователь-владелец</param>
        /// <param name="name">Название списка (макс. 10 символов)</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Созданный список дел</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Выбрасывается при превышении длины имени или если имя уже существует
        /// </exception>
        public async Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 10)
                throw new ArgumentOutOfRangeException(nameof(name), $"Длина названия \"{name}\" должна быть от 1 до 10 символов");

            if (await _toDoListRepository.ExistsByName(user.UserId, name, ct))
                throw new ArgumentException($"Список с названием \"{name}\" уже существует", nameof(name));

            var toDoList = new ToDoList(name, user);
            await _toDoListRepository.Add(toDoList, ct);
            return toDoList;
        }

        /// <summary>
        /// Получает список дел по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор списка</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Найденный список или null</returns>
        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            return await _toDoListRepository.Get(id, ct);
        }

        /// <summary>
        /// Удаляет список дел
        /// </summary>
        /// <param name="id">Идентификатор списка</param>
        /// <param name="ct">Токен отмены</param>
        public async Task Delete(Guid id, CancellationToken ct)
        {
            await _toDoListRepository.Delete(id, ct);
        }

        /// <summary>
        /// Получает все списки указанного пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Неизменяемый список задач пользователя</returns>
        public async Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct)
        {
            return await _toDoListRepository.GetByUserId(userId, ct);
        }
    }
}
