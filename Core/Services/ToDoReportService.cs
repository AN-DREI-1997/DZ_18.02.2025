using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Entities;

namespace DZ_18._02._2025.Core.Services
{
    internal class ToDoReportService : IToDoReportService
    {
        private readonly IToDoRepository _toDoRepository;

        public ToDoReportService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }
        // Реализуем метод Find
        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            return _toDoRepository.Find(userId, predicate);
        }

        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            var todos = _toDoRepository.GetAllByUserId(userId);
            return (
                total: todos.Count,
                completed: todos.Count(t => t.State == ToDoItemState.Completed),
                active: todos.Count(t => t.State == ToDoItemState.Active),
                generatedAt: DateTime.UtcNow
            );
        }
    }
}
