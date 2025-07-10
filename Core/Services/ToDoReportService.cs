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
        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            return await _toDoRepository.FindAsync(userId, predicate, cancellationToken);
        }

        public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken)
        {
            var todos = await _toDoRepository.GetActiveByUserIdAsync(userId, cancellationToken);
            return (
                total: todos.Count,
                completed: todos.Count(t => t.State == ToDoItemState.Completed),
                active: todos.Count(t => t.State == ToDoItemState.Active),
                generatedAt: DateTime.UtcNow
            );
        }
    }
}
