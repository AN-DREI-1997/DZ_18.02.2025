using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Services;
using DZ_18._02._2025.Infastructure.DataAccess;
using DZ_18._02._2025.TelegramBot;
using DZ_18._02._2025.TelegramBot.Scenario;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
//using static DZ_18._02._2025.Exceptions.MyCustomException;

namespace DZ_18._02._2025
{
    internal class Program
    {
        private static List<string> listTask = new List<string>(); //лист для хранения задач
        
        static async Task Main()
        {
            // Получаем токен бота из переменных окружения операционной системы
            string token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.User);

            // Проверяем наличие токена
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Токен бота не найден.");
                return;
            }
            // Создаем экземпляр клиента Telegram с указанным токеном
            var botClient = new TelegramBotClient(token);

            // Настраиваем опции приемника обновлений
            var receiverOptions = new ReceiverOptions
            {
                // Уточняем, что принимаем только обновления типа "сообщение"
                AllowedUpdates = new[] { UpdateType.Message },
                // Пропускаем ожидающие обновления, чтобы начать работу с чистого листа
                DropPendingUpdates = true
            };

            string pathFileToDoRep = Path.GetTempPath(); //указать конкретную директорию для хранения задач
            string pathFileUserRep = Path.GetTempPath(); ////указать конкретную директорию для хранения пользователей

            var toDoRep = new FileToDoRepository(pathFileToDoRep); //Репозиторий задач
            var userRep = new FileUserRepository(pathFileUserRep); //репозиторий пользователей

            var userService = new UserService(userRep);
            var toDoService = new ToDoService(toDoRep);

           // IToDoListService toDoListService = new ToDoListService(toDoListRepository);

            IScenario[] scenarios = new IScenario[]
           {
                new AddTaskScenario(userService, toDoService),
           };

            var scenarioContextRepository = new InMemoryScenarioContextRepository();

            var handler = new UpdateHandler(botClient, userService, toDoService, toDoRep,scenarios,scenarioContextRepository);

            try
            {
                handler.UpdateStarted += handler.OnHandleUpdateStarted;
                handler.UpdateCompleted += handler.OnHandleUpdateCompleted;
                using CancellationTokenSource cts = new();

                // Начинаем получать обновления от Telegram сервера
                botClient.StartReceiving(handler.HandleUpdateAsync, handler.HandleErrorAsync, receiverOptions, cts.Token);

                // Запрашиваем информацию о нашем боте
                var me = await botClient.GetMe();
                Console.WriteLine($"{me.Username} запущен"); // Сообщаем, что бот успешно запустился
                Console.WriteLine("Нажмите A для выхода."); // Инструкция для остановки бота

                // Блок бесконечного цикла, ожидающего нажатия клавиши 'A' для выхода
                while (true)
                {
                    var key = Console.ReadKey(true); // Читаем символ клавиатуры без отображения
                    if (key.KeyChar == 'a')          // Если нажата клавиша 'A'
                    {
                        cts.Cancel();                 // Останавливаем процесс получения обновлений
                        break;                        // Выходим из цикла
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Произошла непредвиденная ошибка:\n {ex.GetType()}\n {ex.Message}\n {ex.StackTrace}\n {ex.InnerException}");
            }
            finally
            {
                // После завершения подписки удаляем регистрацию событий
                handler.UpdateStarted -= handler.OnHandleUpdateStarted;
                handler.UpdateCompleted -= handler.OnHandleUpdateCompleted;
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
