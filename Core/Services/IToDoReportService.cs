using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZ_18._02._2025.Core.Entities;

namespace DZ_18._02._2025.Core.Services
{
    interface IToDoReportService
    {

        Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken);
   

        (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId);

    }
}
