using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Services;
using DZ_18._02._2025.Infastructure.DataAccess;
using DZ_18._02._2025.TelegramBot;
using DZ_18._02._2025.TelegramBot.Scenario;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DZ_18._02._2025
{
    internal class Program
    {     
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

            var toDoRepository = new FileToDoRepository(Path.Combine(Directory.GetCurrentDirectory(), "FileToDoRepository"));
            IToDoService toDoService = new ToDoService(toDoRepository);

            var userRepository = new FileUserRepository(Path.Combine(Directory.GetCurrentDirectory(), "FileUserRepository"));
            IUserService userService = new UserService(userRepository);

            IToDoListRepository toDoListRepository = new FileToDoListRepository(Path.Combine(Directory.GetCurrentDirectory(), "FileToDoListRepository"));
            IToDoListService toDoListService = new ToDoListService(toDoListRepository);

            IScenarioContextRepository contextRepository = new InMemoryScenarioContextRepository();

            IScenario[] scenarios =
           [
                 new AddTaskScenario(userService, toDoService, toDoListService),
                new AddListScenario(userService, toDoListService),
                new DeleteListScenario(userService, toDoListService, toDoService)
           ];

            var scenarioContextRepository = new InMemoryScenarioContextRepository();

            var handler = new UpdateHandler(botClient, userService, toDoService, toDoRepository,scenarios,scenarioContextRepository);

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
    }
}
