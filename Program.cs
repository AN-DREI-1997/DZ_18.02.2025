using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Services;
using DZ_18._02._2025.Infastructure.DataAccess;
using DZ_18._02._2025.TelegramBot;
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
            // Получаем токен бота из переменных окружения операционной системы
            string token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
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

            var toDoRep = new InMemoryToDoRepository(); //Репозиторий задач
            var userRep = new InMemoryUserRepository(); //репозиторий пользователей

            var userService = new UserService(userRep);
            var toDoService = new ToDoService(toDoRep, maxTaskCount, maxTaskLenght);

            // Создаем обработчик, передавая зависимости через конструктор
            //IUpdateHandler handler = new TelegramBot.UpdateHandler(botClient, userService, toDoService,toDoRep, maxTaskCount, maxTaskLenght);

            var handler = new UpdateHandler(botClient, userService, toDoService, toDoRep);
            // Подписываемся на события начала и завершения обработки обновлений
            
            

            void OnProcessingStarted(string message)
            {
                handler.OnHandleUpdateStarted += msg => Console.WriteLine($"Началась обработка сообщения '{msg}'");
                Console.WriteLine($"Началась обработка сообщения '{message}'.");
            }

            void OnProcessingFinished(string message)
            {
               handler.OnHandleUpdateCompleted += msg => Console.WriteLine($"Закончена обработка сообщения '{msg}'");
            }

            // Используем объект для отмены операции, если потребуется остановка
            using CancellationTokenSource cts = new();

            try
            {
                // Регистрируем доступные команды для нашего бота
                await botClient.SetMyCommands(new BotCommand[]
                {
                new BotCommand("/start", "Начало работы"),     // Стартовая команда
                new BotCommand("/help", "Справка по командам"),// Справочная команда
                new BotCommand("/info", "Информация о сервисе"),// Команда для вывода инфо о сервисе
                new BotCommand("/addtask", "Добавить задачу"), // Добавление новой задачи
                new BotCommand("/showtasks", "Просмотреть активные задачи"),// Просмотр текущих задач
                new BotCommand("/showalltasks", "Просмотреть все задачи"),// Просмотр всех задач
                new BotCommand("/removetask", "Удалить задачу"),// Удаление задачи
                new BotCommand("/completetask", "Завершить задачу"),// Завершение задачи
                new BotCommand("/report", "Получить отчет по задачам"),// Формирование отчета
                new BotCommand("/find", "Найти задачу по названию")// Поиск задачи по имени
                });

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
            finally
            {
                // После завершения подписки удаляем регистрацию событий
                handler.OnHandleUpdateStarted -= msg => Console.WriteLine($"Началась обработка сообщения '{msg}'");
                handler.OnHandleUpdateCompleted -= msg => Console.WriteLine($"Закончена обработка сообщения '{msg}'");
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
