using DZ_18._02._2025.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DZ_18._02._2025.Core.DadaAccess
{
    public interface IToDoListRepository
    {
        /// <summary>
        /// Получает конкретный список дел по его идентификатору
        /// </summary>
        /// <param name="id">Идентификатор списка дел</param>
        /// <param name="ct">Токен отмены операции</param>
        /// <returns>Найденный список дел или null, если не найден</returns>
        Task<ToDoList?> Get(Guid id, CancellationToken ct);
        /// <summary>
        /// Получает все списки дел для указанного пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// /// <param name="ct">Токен отмены операции</param>
        /// <returns>Неизменяемый список всех списков дел пользователя</returns>
        Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct);
        /// <summary>
        /// Добавляет новый список дел в хранилище
        /// </summary>
        /// <param name="list">Список дел для добавления</param>
        /// <param name="ct">Токен отмены операции</param>
        /// <remarks>
        /// Создает папку пользователя если она не существует,
        /// сохраняет список в формате JSON в файл с именем {Id_списка}.json
        /// </remarks>
        /// <exception cref="ArgumentNullException">Если передан null вместо списка</exception>
        /// <exception cref="IOException">При ошибках записи файла</exception>
        Task Add(ToDoList list, CancellationToken ct);
        /// <summary>
        /// Удаляет список дел по указанному идентификатору
        /// </summary>
        /// <param name="id">Идентификатор списка дел для удаления</param>
        /// <param name="ct">Токен отмены операции</param>
        /// <exception cref="FileNotFoundException">Если файл списка дел не найден</exception>
        /// <remarks>
        /// Метод сначала проверяет существование списка через Get(), 
        /// затем удаляет соответствующий JSON-файл из папки пользователя
        /// </remarks>
        Task Delete(Guid id, CancellationToken ct);
        /// <summary>
        /// Проверяет, существует ли список дел с указанным именем у данного пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="name">Имя списка дел для проверки</param>
        /// <param name="ct">Токен отмены операции</param>
        /// <returns>True если список с таким именем уже существует у пользователя, иначе False</returns>
        Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct);
    }
}
