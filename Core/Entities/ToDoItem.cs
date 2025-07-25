﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DZ_18._02._2025.Core.Entities
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
        public DateTime CreatedAt { get; private set; } 
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; } 

        
    }
}
