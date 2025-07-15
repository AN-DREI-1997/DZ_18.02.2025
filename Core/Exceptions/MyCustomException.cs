using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DZ_18._02._2025.Core.Exceptions
{
    public class MyCustomException: Exception
    {
        public class TaskCountLimitException : Exception
        {
            /// <summary>
            /// Выдает исключение при привышении максимального заданного количества задач!
            /// Принимает параметры:
            /// <see langword="int taskCountLimit"/>
            /// </summary>
            public TaskCountLimitException(int taskCountLimit)
                : base($"Превышено максимальное количество задач равное {taskCountLimit}")
            {
            }
        }

        public class TaskLengthLimitException : Exception
        {
            /// <summary>
            /// Выдает исключение при привышении максимальной длины задачи! 
            /// Принимает параметры:
            /// <see langword="int taskLengthLimit"/>
            /// </summary>
            public TaskLengthLimitException(int taskLengthLimit)
                : base($"Длина задачи не должна превышать максимально допустимое значение: {taskLengthLimit}")
            {
            }
        }

        public class DuplicateTaskException : Exception
        {
            /// <summary>
            /// Выдает исключение при дублировании задач! 
            /// Принимает параметры:
            /// <see langword=" string task"/>
            /// </summary>
            public DuplicateTaskException(string task)
                : base($"Задача '{task}' уже существует")
            {
            }
        }

    }
}
