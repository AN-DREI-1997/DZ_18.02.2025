using DZ_18._02._2025.Core.Entities;
using System.Text.Json;

namespace DZ_18._02._2025.Core.DadaAccess
{
    internal class FileToDoRepository : IToDoRepository
    {
        private readonly string _baseDirectory;

        public FileToDoRepository(string baseDirectory)
        {
            _baseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));

            // Создаем базовую директорию, если она не существует
            Directory.CreateDirectory(_baseDirectory);
        }
        private string GetFilePath(Guid id) => Path.Combine(_baseDirectory, $"{id}.json");

        public async Task AddAsync(ToDoItem item, CancellationToken cancellationToken)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var filePath = GetFilePath(item.Id);

            await using var fileStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fileStream, item, cancellationToken: cancellationToken);
        }

        public async Task<int> CountActiveAsync(Guid userId, CancellationToken cancellationToken)
        {
            var activeItems = await GetActiveByUserIdAsync(userId, cancellationToken);
            return activeItems.Count;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var filePath = GetFilePath(id);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken cancellationToken)
        {
            var items = await FindAsync(userId, item =>
            string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase), cancellationToken);
            return items.Any();
        }

        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            var result = new List<ToDoItem>();

            foreach (var filePath in Directory.EnumerateFiles(_baseDirectory, "*.json"))
            {
                try
                {
                    await using var fileStream = File.OpenRead(filePath);
                    var item = await JsonSerializer.DeserializeAsync<ToDoItem>(fileStream, cancellationToken: cancellationToken);

                    if (item != null && item.User.UserId == userId && predicate(item))
                    {
                        result.Add(item);
                    }
                }
                catch (Exception)
                {
                    // Пропускаем файлы, которые не удалось прочитать или десериализовать
                    continue;
                }
            }

            return result;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await FindAsync(userId, item => item.State == ToDoItemState.Active, cancellationToken);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await FindAsync(userId, _ => true, cancellationToken);
        }

        public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            var filePath = GetFilePath(id);
            if (!File.Exists(filePath)) return null;

            await using var fileStream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<ToDoItem>(fileStream, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(ToDoItem item, CancellationToken cancellationToken)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var filePath = GetFilePath(item.Id);
            if (!File.Exists(filePath)) throw new FileNotFoundException("ToDoItem not found", filePath);

            item.StateChangedAt = DateTime.UtcNow;

            await using var fileStream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(fileStream, item, cancellationToken: cancellationToken);
        }
    }
}
