using System.Text;
using System.Text.RegularExpressions;
using DZ_18._02._2025.Services;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
//using static DZ_18._02._2025.Exceptions.MyCustomException;

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
            // Создаем экземпляры сервисов через интерфейсы
            IUserService userService = new UserService();
            IToDoService toDoService = new ToDoService();
            ITelegramBotClient botClient = new ConsoleBotClient();
            // Создаем обработчик, передавая зависимости через конструктор
            IUpdateHandler handler = new UpdateHandler(botClient, userService, toDoService);

            try
            {
                botClient.StartReceiving(handler);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }
        }

    }
}
