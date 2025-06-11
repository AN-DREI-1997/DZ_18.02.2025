using System.Text;
using System.Text.RegularExpressions;
using static DZ_18._02._2025.MyCustomException;

namespace DZ_18._02._2025
{
    internal class Program
    {
        private static List<string> listTask = new List<string>(); //лист для хранения задач
        private static int maxTaskCount;
        private static int maxTaskLenght;
        static string userName = string.Empty;
        static void Main(string[] args)
        {
            try
            {
                bool isRunning = true;
                bool isCountTask = true;
                bool isLengthLimitTask = true;

                Console.WriteLine("Добро пожаловать в нашего бота! Доступные команды: /start, /help, /info, /echo, /addtask, /showtasks, /removetask, /exit\n");
                while (isCountTask)
                {
                    try
                    {
                        Console.WriteLine("Введите максимально допустимое количество задач (от 1 до 100):");
                        string input = Console.ReadLine();
                        maxTaskCount = ValidateTaskCount(input);
                        isCountTask = false;
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                while (isLengthLimitTask)
                {
                    try
                    {
                        Console.WriteLine("Введите максимально допустимую длину задачи (от 1 до 100):");
                        string input = Console.ReadLine();
                        maxTaskLenght = ValidateTaskCount(input);
                        isLengthLimitTask = false;
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                while (isRunning)
                {
                    Console.Write("Введите команду: ");
                    string? input = Console.ReadLine();
                    if (input.Equals("/start", StringComparison.OrdinalIgnoreCase))
                    {
                        Start();
                    }
                    else if (input.Equals("/help", StringComparison.OrdinalIgnoreCase))
                    {
                        Help();
                    }
                    else if (input.Equals("/info", StringComparison.OrdinalIgnoreCase))
                    {
                        Info();
                    }
                    else if (input.Equals("/echo", StringComparison.OrdinalIgnoreCase))
                    {
                        Echo();
                    }
                    else if (input.Equals("/addtask", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            AddTask();
                        }
                        catch (TaskCountLimitException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        catch (TaskLengthLimitException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        catch (ArgumentException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    else if (input.Equals("/exit", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Спасибо за использование нашего бота! До свидания!");
                        isRunning = false;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Неизвестная команда. Пожалуйста, попробуйте снова.\nИли используйте команду /help для вызова корректных команд.");
                    }
                }
            }
            catch
            {

            }
        }

        private static void Echo()
        {
            if (string.IsNullOrEmpty(userName))
            {
                Console.WriteLine("Сначала введите команду /start и ваше имя.");
            }
            else
            {
                Console.Write($"{userName}, введите текст для повторения: ");
                string? echoText = Console.ReadLine();
                // Удаляем подстроку "/echo" из введенного текста
                string cleanedText = echoText.Replace("/echo", string.Empty).Trim();
                Console.WriteLine($"{userName}, Вы сказали: {cleanedText}");
            }
        }

        private static void Start()
        {
            while (true)
            {
                Console.Write("Пожалуйста, введите ваше имя пользователя: ");
                userName = Console.ReadLine();

                if (IsvalidUserName(userName))
                {
                    Console.WriteLine($"Привет, {userName}! Как я могу помочь вам сегодня?");
                    break; // Выход из цикла, если имя пользователя валидно
                }
                else
                {
                    Console.WriteLine("Имя пользователя некорректно. Пожалуйста, используйте только буквы и длину от 3 до 20 символов.");
                }
            }
        }

        private static void Help()
        {
            Console.WriteLine("Справка: Используйте команды: ");
            Console.WriteLine("/start - для начала работы с ботом;");
            Console.WriteLine("info - для информации о программе;");
            Console.WriteLine("/echo - для повторения введенного текста;");
            Console.WriteLine("/addtask - добавить задачу в список;");
            Console.WriteLine("/showtasks - показать задачи из списка;");
            Console.WriteLine("/removetask - удалить задачу из списка;");
            Console.WriteLine("/exit - для выхода;");
        }

        private static void Info()
        {
            Console.WriteLine("Версия программы: 1.0.\nДата создания: Февраль 2025.");
        }

        public static void AddTask()
        {
            while (true)
            {
                try
                {
                    int currentTaskCount = listTask.Count; // текущее количество задач
                    if (currentTaskCount >= maxTaskCount)
                    {
                        throw new TaskCountLimitException(maxTaskCount);
                    }

                    Console.WriteLine("Пожалуйста, введите описание задачи: ");
                    string? newTask = Console.ReadLine();
                    ValidateString(newTask);
                    if (!string.IsNullOrEmpty(newTask))
                    {

                        if (newTask.Length <= maxTaskLenght)
                        {
                            listTask.Add(newTask);
                            Console.WriteLine("Задача добавлена: " + newTask);
                            break; // Выход из цикла, если задача успешно добавлена
                        }
                        else
                        {
                            throw new TaskLengthLimitException(newTask.Length, maxTaskLenght);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Описание задачи не может быть пустым. Пожалуйста, попробуйте снова.");
                    }
                }
                catch (TaskCountLimitException ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Пожалуйста, удалите одну из существующих задач, чтобы добавить новую.");
                    ShowTasks(); // Показываем текущие задачи
                    RemoveTask(); // Предлагаем удалить задачу
                }
            }
        }
        public static void ShowTasks()
        {
            if (listTask.Count != 0)
            {
                // Отображаем список задач
                var stringBuilder_listTask = new StringBuilder("Вот ваш список задач:\n");
                for (int i = 0; i < listTask.Count; i++)
                {
                    stringBuilder_listTask.AppendLine($"{i + 1}. {listTask[i]}");
                }
                Console.WriteLine(stringBuilder_listTask.ToString());
            }
            else
            {
                Console.WriteLine("Список задач пуст.");
                return;
            }
        }
        public static void RemoveTask()
        {
            if (listTask.Count == 0)
            {
                Console.WriteLine("Невозможно удалить задачу, список пуст.");
                return;
            }
            else
            {
                ShowTasks();
                Console.WriteLine("введите номер задачи для удаления:");
                if (int.TryParse(Console.ReadLine(), out int taskNumber))
                {
                    if (taskNumber > 0 && taskNumber <= listTask.Count)
                    {
                        listTask.RemoveAt(taskNumber - 1);
                        Console.WriteLine($"Задача {taskNumber} удалена.");
                    }
                    else
                    {
                        Console.WriteLine("Введите корректный номер задачи, который требуется удалить!");
                    }
                }
                else
                {
                    Console.WriteLine("Некорректный ввод. Введите число.");
                }
            }
        }
        static bool IsvalidUserName(string username)
        {
            // Регулярное выражение для проверки имени пользователя
            string pattern = @"^[a-zA-Z]{3,20}$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(username);
        }


        private static int ValidateTaskCount(string input)
        {
            if (!int.TryParse(input, out int taskCount) || taskCount < 1 || taskCount > 100)
            {
                throw new ArgumentException("Введите корректное число от 1 до 100.");
            }
            return taskCount;
        }
        public static void ValidateString(string? str)
        {
            // Проверка на null
            if (str == null)
            {
                throw new ArgumentException("Строка не должна быть null.");
            }

            // Проверка на пустую строку
            if (str.Trim().Length == 0)
            {
                throw new ArgumentException("Строка не должна быть пустой или состоять только из пробелов.");
            }

            // Если все проверки пройдены, строка валидна
            Console.WriteLine("Строка валидна: " + str);
        }
    }
}
