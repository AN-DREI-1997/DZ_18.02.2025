
//Описание/Пошаговая инструкция выполнения домашнего задания:
//Вам предстоит создать консольное приложение, которое будет имитировать интерактивное меню бота согласно следующему плану:

//Приветствие: При запуске программы отображается сообщение приветствия со списком доступных команд: / start, / help, / info, / exit.
//Обработка команды /start: Если пользователь вводит команду /start, программа просит его ввести своё имя. Сохраните введенное имя в переменную.
//Программа должна обращаться к пользователю по имени в каждом следующем ответе.
//Обработка команды /help: Отображает краткую справочную информацию о том, как пользоваться программой.
//Обработка команды /info: Предоставляет информацию о версии программы и дате её создания.
//Доступ к команде /echo: После ввода имени становится доступной команда /echo. При вводе этой команды с аргументом (например, /echo Hello),
//программа возвращает введенный текст (в данном примере "Hello").
//Основной цикл программы: Программа продолжает ожидать ввод команды от пользователя, пока не будет введена команда /exit.

using System.Text;

namespace DZ_18._02._2025
{
    internal class Program
    {
        private static List<string> listTask = new List<string>(); //лист для хранения задач
        static void Main(string[] args)
        {
            string userName = string.Empty;
            bool isRunning = true;

            Console.WriteLine("Добро пожаловать в нашего бота! Доступные команды: /start, /help, /info, /echo, /addtask, /showtasks, /removetask, /exit");

            while (isRunning)
            {
                Console.Write("Введите команду: ");
                string input = Console.ReadLine();
                switch (input.ToLower())
                {
                    case "/start":
                        if (string.IsNullOrEmpty(userName))
                        {
                            Console.Write("Пожалуйста, введите ваше имя: ");
                            userName = Console.ReadLine();
                            Console.WriteLine($"Привет, {userName}! Как я могу помочь вам сегодня?");
                        }
                        else
                        {
                            Console.WriteLine($"С возвращением, {userName}!");
                        }
                        break;
                    
                    case "/help":
                        Console.WriteLine("Справка: Используйте команды: ");
                        Console.WriteLine("/start - для начала работы с ботом;");
                         Console.WriteLine("info - для информации о программе;");
                         Console.WriteLine("/echo - для повторения введенного текста;");
                         Console.WriteLine("/addtask - добавить задачу в список;");
                         Console.WriteLine("/showtasks - показать задачи из списка;");
                         Console.WriteLine("/removetask - удалить задачу из списка;");
                        Console.WriteLine("/exit - для выхода;");
                        break;

                    case "/info":
                        Console.WriteLine("Версия программы: 1.0.\nДата создания: Февраль 2025.");
                        break;

                    case "/echo":
                        if (string.IsNullOrEmpty(userName))
                        {
                            Console.WriteLine("Сначала введите команду /start и ваше имя.");
                        }
                        else
                        {
                            Console.Write($"{userName}, введите текст для повторения: ");
                            string echoText = Console.ReadLine();
                            // Удаляем подстроку "/echo" из введенного текста
                            string cleanedText = echoText.Replace("/echo", string.Empty).Trim();
                            Console.WriteLine($"{userName}, Вы сказали: {cleanedText}");
                        }
                        break;

                    case "/addtask":
                        AddTask();
                        break;

                    case "/showtasks":
                        ShowTasks();
                        break;

                    case "/removetask":
                        RemoveTask();
                        break;

                    case "/exit":
                        Console.WriteLine("Спасибо за использование нашего бота! До свидания!");
                        isRunning = false;
                        break;

                    default:
                        Console.WriteLine("Неизвестная команда. Пожалуйста, попробуйте снова.\nИли используйте команду /help для вызова корректных команд.");
                        break;
                }
            }

        }
        public static void AddTask()
        {
            Console.WriteLine("Пожалуйста, введите описание задачи: ");
            string newTask = Console.ReadLine();
            if (!string.IsNullOrEmpty(newTask))
            {
                listTask.Add(newTask);
                Console.WriteLine("Задача добавлена: " + newTask);
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
    }
}
