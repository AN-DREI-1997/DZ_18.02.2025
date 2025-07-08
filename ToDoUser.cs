using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DZ_18._02._2025.Entities
{
    public class ToDoUser
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
        public long TelegramUserId { get; set; }
        public string? TelegramUserName { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}
