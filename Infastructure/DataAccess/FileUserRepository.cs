using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DZ_18._02._2025.Infastructure.DataAccess
{
    internal class FileUserRepository : IUserRepository
    {
        private readonly string _baseDirectory;

        public FileUserRepository(string baseDirectory)
        {
            _baseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));

            // Создаем базовую директорию, если она не существует
            Directory.CreateDirectory(_baseDirectory);
        }

        private string GetFilePath(Guid userId) => Path.Combine(_baseDirectory, $"{userId}.json");

        public async Task AddAsync(ToDoUser user, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            user.RegisteredAt = DateTime.UtcNow;
            var filePath = GetFilePath(user.UserId);

            await using var fileStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fileStream, user, cancellationToken: cancellationToken);
        }
        public async Task<ToDoUser?> GetUserAsync(long userId, CancellationToken cancellationToken)
        {
            foreach (var filePath in Directory.EnumerateFiles(_baseDirectory, "*.json"))
            {
                try
                {
                    await using var fileStream = File.OpenRead(filePath);
                    var user = await JsonSerializer.DeserializeAsync<ToDoUser>(fileStream, cancellationToken: cancellationToken);

                    if (user != null && user.TelegramUserId == userId)
                    {
                        return user;
                    }
                }
                catch (Exception)
                {
                    // Пропускаем файлы, которые не удалось прочитать или десериализовать
                    continue;
                }
            }

            return null;
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId)
        {
            foreach (var filePath in Directory.EnumerateFiles(_baseDirectory, "*.json"))
            {
                try
                {
                    await using var fileStream = File.OpenRead(filePath);
                    var user = await JsonSerializer.DeserializeAsync<ToDoUser>(fileStream);

                    if (user != null && user.TelegramUserId == telegramUserId)
                    {
                        return user;
                    }
                }
                catch (Exception)
                {
                    // Пропускаем файлы, которые не удалось прочитать или десериализовать
                    continue;
                }
            }

            return null;
        }
    }
}
