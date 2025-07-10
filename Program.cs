using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Services;
using DZ_18._02._2025.Infastructure.DataAccess;
using DZ_18._02._2025.TelegramBot;
using Otus.ToDoList.ConsoleBot;
//using static DZ_18._02._2025.Exceptions.MyCustomException;

namespace DZ_18._02._2025
{
    internal class Program
    {
        private static List<string> listTask = new List<string>(); //лист для хранения задач
        private static int maxTaskCount;
        private static int maxTaskLenght;
        static async Task Main()
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
            //IUpdateHandler handler = new TelegramBot.UpdateHandler(botClient, userService, toDoService,toDoRep, maxTaskCount, maxTaskLenght);

            UpdateHandler handler = new UpdateHandler(botClient, userService, toDoService, toDoRep, maxTaskCount, maxTaskLenght);
            // Подписываемся на события начала и завершения обработки обновлений
            handler. OnHandleUpdateStarted += msg => Console.WriteLine($"Началась обработка сообщения '{msg}'");
            handler.OnHandleUpdateCompleted += msg => Console.WriteLine($"Закончена обработка сообщения '{msg}'");

            void OnProcessingStarted(string message)
            {
                Console.WriteLine($"Началась обработка сообщения '{message}'.");
            }

            void OnProcessingFinished(string message)
            {
                Console.WriteLine($"Закончена обработка сообщения '{message}'.");
            }

            var cts = new CancellationTokenSource();

            // Отписываемся от событий
            Console.CancelKeyPress += (_, _) =>
            {
                cts.Cancel();
                handler.OnHandleUpdateStarted -= OnProcessingStarted;
                handler.OnHandleUpdateCompleted -= OnProcessingFinished;
                cts.Dispose();
            };
            try
            {
                botClient.StartReceiving(handler, cts.Token); //Приём сообщений
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }
            await Task.Delay(-1, cts.Token);
            // После завершения подписки удаляем регистрацию событий
            handler.OnHandleUpdateStarted -= msg => Console.WriteLine($"Началась обработка сообщения '{msg}'");
            handler.OnHandleUpdateCompleted -= msg => Console.WriteLine($"Закончена обработка сообщения '{msg}'");
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
