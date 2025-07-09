using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Services;
using DZ_18._02._2025.Infastructure.DataAccess;
using Otus.ToDoList.ConsoleBot;
//using static DZ_18._02._2025.Exceptions.MyCustomException;

namespace DZ_18._02._2025
{
    internal class Program
    {
        private static List<string> listTask = new List<string>(); //лист для хранения задач
        private static int maxTaskCount;
        private static int maxTaskLenght;
        static void Main(string[] args)
        {
            bool isCountTask = true;
            bool isLengthLimitTask = true;

            while (isCountTask)
            {
                try
                {
                    Console.WriteLine("Введите максимально допустимое количество задач (от 1 до 100):");
                    string? input1 = Console.ReadLine();
                    maxTaskCount = ValidateTaskCount(input1);
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
                    string? input2 = Console.ReadLine();
                    maxTaskLenght = ValidateTaskCount(input2);
                    isLengthLimitTask = false;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            ITelegramBotClient botClient = new ConsoleBotClient();

            var toDoRep = new InMemoryToDoRepository(); //Репозиторий задач
            var userRep = new InMemoryUserRepository(); //репозиторий пользователей

            var userService = new UserService(userRep);
            var toDoService = new ToDoService(toDoRep);

            // Создаем обработчик, передавая зависимости через конструктор
            IUpdateHandler handler = new TelegramBot.UpdateHandler(botClient, userService, toDoService,toDoRep, maxTaskCount, maxTaskLenght);

            try
            {
                botClient.StartReceiving(handler);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }
        }
        private static int ValidateTaskCount(string input)
        {
            if (!int.TryParse(input, out int taskCount) || taskCount < 1 || taskCount > 100)
            {
                throw new ArgumentException("Введите корректное число от 1 до 100.");
            }
            return taskCount;
        }
    }
}
