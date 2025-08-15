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
    internal class FileToDoListRepository : IToDoListRepository
    {
        private string _baseDirecory;

        public FileToDoListRepository(string baseFolder)
        {
            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }
            _baseDirecory = baseFolder;

        }
        
        public async Task Add(ToDoList list, CancellationToken ct)
        {
            var userFolder = Path.Combine(_baseDirecory, list.ToDoUser.UserId.ToString());
            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }
            var filePath = Path.Combine(userFolder, $"{list.Id}.json");
            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            var toDoList = await Get(id, ct);
            var filePath = Path.Combine(_baseDirecory, toDoList.ToDoUser.UserId.ToString(), $"{id.ToString()}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            return (await GetByUserId(userId, ct)).Any(i => i.Name == name);
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            var filePath = Path.Combine(_baseDirecory, $"{id}.json");

            if (!File.Exists(filePath))
                return null;

            try
            {
                var json = await File.ReadAllTextAsync(filePath, ct);
                return JsonSerializer.Deserialize<ToDoList>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {filePath}:\n{ex.Message}");
                return null;
            }
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            var userFolder = Path.Combine(_baseDirecory, userId.ToString());

            if (!Directory.Exists(userFolder))
                return  Array.Empty<ToDoList>().AsReadOnly();

            var result = new List<ToDoList>();

            foreach (var file in Directory.EnumerateFiles(userFolder, "*.json"))
            {
                try
                {
                    var list = JsonSerializer.Deserialize<ToDoList>(File.ReadAllText(file));
                    if (list?.ToDoUser?.UserId == userId)
                        result.Add(list);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {file}:\n{ex.Message}");
                }
            }

            return result.AsReadOnly();
        }
    }
}
