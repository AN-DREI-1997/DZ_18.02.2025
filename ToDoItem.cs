using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DZ_18._02._2025.Entities
{
    public enum ToDoItemState
    {
        Active,
        Completed
    }
    public class ToDoItem(ToDoUser user, string name)
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public ToDoUser User { get; set; } = user;
        public string Name { get; set; } = name;
        public DateTime CreatedAt { get; private set; } = DateTime.Now;
        public ToDoItemState State { get; set; } = ToDoItemState.Active;
        public DateTime? StateChangedAt { get; set; } = null;

        public void CompleteTask()
        {
            State = ToDoItemState.Completed;
            StateChangedAt = DateTime.Now;
        }
    }
}
