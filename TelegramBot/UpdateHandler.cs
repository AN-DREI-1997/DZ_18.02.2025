
using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Entities;
using DZ_18._02._2025.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static DZ_18._02._2025.Core.Exceptions.MyCustomException;

namespace DZ_18._02._2025.TelegramBot
{
    internal class UpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        private readonly IToDoRepository _toDoRepository; // Добавляем репозиторий задач


        // Делегаты и события
        public delegate void MessageEventHandler(string message);
        public event MessageEventHandler? OnHandleUpdateStarted;
        public event MessageEventHandler? OnHandleUpdateCompleted;

        // Метод для экранирования спецсимволов Markdown
        private string EscapeMarkdownCharacters(string input)
        {
            return input
                .Replace("\\", "\\\\")          // Экранируем сам символ \
                .Replace("-", "\\-");           // Экранируем дефис -
        }

        public UpdateHandler(ITelegramBotClient botClient, IUserService userService, IToDoService toDoService, IToDoRepository toDoRepository)

        {
            _botClient = botClient;
            _userService = userService;
            _toDoService = toDoService;
            _toDoRepository = toDoRepository;

        }
       
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                // Событие начала обработки
                OnHandleUpdateStarted?.Invoke(update.Message.Text);

                await ProcessMessage(botClient, update.Message, cancellationToken);

                // Событие окончания обработки
                OnHandleUpdateCompleted?.Invoke(update.Message.Text);
            }
        }

        private async Task ProcessMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            ToDoUser? user = await _userService.GetUserAsync(message.From.Id, cancellationToken);

            if (user == null)
            {
                await SendStartMenu(botClient, message.Chat.Id, cancellationToken);
            }
            else
            {
                await SendRegisteredMenu(botClient, message.Chat.Id, cancellationToken);
            }

            await ProcessCommand(botClient, message, cancellationToken);
        }

        private async Task SendRegisteredMenu(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            await botClient.SendMessage(chatId, "Выберите нужную команду:",
             replyMarkup: KeyboardHelper.CreateRegisteredButtons(), cancellationToken: cancellationToken);
        }

        private async Task SendStartMenu(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            await botClient.SendMessage(chatId, "Для начала работы нажмите /start",
            replyMarkup: KeyboardHelper.CreateStartButton(), cancellationToken: cancellationToken);
        }

        // Реализация метода HandleErrorAsync
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка при обработке обновления: {exception.Message}");

            // Мы можем уведомить пользователя, используя информацию, если доступна такая возможность
            if (exception is Telegram.Bot.Exceptions.ApiRequestException apiEx && !string.IsNullOrEmpty(apiEx.Message))
            {
                await botClient.SendMessage(-1 /* Здесь укажите реальный Chat ID */, "Возникла внутренняя ошибка. Попробуйте позже.", cancellationToken: cancellationToken);
            }
            else
            {
                Console.WriteLine("Сообщение об ошибке невозможно передать пользователю.");
            }
        }

        private async Task ProcessCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Text.StartsWith('/'))
            {
                var parts = message.Text.Split(' ', 2);
                string cmd = parts.Length > 0 ? parts[0].ToLower() : "";
                string? arg = parts.Length > 1 ? parts[1].Trim() : null;

                // Получаем пользователя
                ToDoUser? user = await _userService.GetUserAsync(message.From.Id, cancellationToken);

                // Регистрация пользователя при команде /start
                if (cmd == "/start")
                {
                    if (user == null)
                    {
                        user = await _userService.RegisterUserAsync(message.From.Id, message.From.Username ?? "", cancellationToken);
                       await _botClient.SendMessage(message.Chat, $"Привет, {user.TelegramUserName}! Ты успешно зарегистрирован.", cancellationToken: cancellationToken);
                    }
                    else
                    {
                       await _botClient.SendMessage(message.Chat, $"Привет, {user.TelegramUserName}!", cancellationToken: cancellationToken);
                    }
                    return;
                }
                // Незарегистрированным пользователям доступно только /help и /info
                if (user == null)
                {
                    if (cmd == "/help" || cmd == "/info")
                    {
                       await ExecutePublicCommand(cmd, message, user,cancellationToken);
                    }
                    else
                    {
                       await _botClient.SendMessage(message.Chat, "Вы не зарегистрированы. Для использования программы зарегистрируйтесь командой /start.\n" +
                                                            "Доступны только команды:\n" +
                                                            "/help - показать помощь\n" +
                                                            "/info - получить информацию о приложении", cancellationToken: cancellationToken);
                    }
                    return;
                }
                // Зарегистрированным пользователям доступны все команды
                switch (cmd)
                {
                    case "/help":
                        await CmdHelp(message, user, cancellationToken);
                        break;
                    case "/info":
                        await CmdInfo(message, user, cancellationToken);
                        break;
                    case "/addtask":
                        await CmdAddTask(message, user, arg, cancellationToken);
                       break;
                    case "/showtasks":
                        await CmdShowTasks(message, user, cancellationToken);
                        break;
                    case "/removetask":
                        await CmdRemoveTask(message, user, arg, cancellationToken);
                        break;
                    case "/completetask":
                        await CmdCompleteTask(message, user, arg, cancellationToken);
                        break;
                    case "/showalltasks":
                        await CmdShowAllTasks(message, user, cancellationToken);
                        break;
                    case "/report": // Добавляем обработку команды /report
                        await CmdReport(message, user, cancellationToken);
                        break;
                    case "/find": // Добавляем обработку команды /find
                        await CmdFind(message, user, arg, cancellationToken);
                        break;
                    default:
                        await  _botClient.SendMessage(message.Chat, "Команды не найдено. Используйте /help для помощи.", cancellationToken: cancellationToken);
                        break;
                }
            }
            else
            {
                // Сообщение не начинается с '/', выводим подсказку
               await _botClient.SendMessage(message.Chat, "Команда должна начинаться с символа '/'", cancellationToken: cancellationToken);
            }
        }

        private async Task CmdFind(Message message, ToDoUser user, string? arg, CancellationToken cancellationToken)
        {
            if (arg == null)
            {
                await _botClient.SendMessage(message.Chat, "Введи текст для поиска после команды /find.", cancellationToken: cancellationToken);
                return;
            }

            var foundTodos = await _toDoService.FindAsync(user, arg,cancellationToken);
            if (foundTodos.Any())
            {
                foreach (var todo in foundTodos)
                {
                    await _botClient.SendMessage(message.Chat, $"{todo.Name} - {todo.CreatedAt} - {todo.Id}", cancellationToken: cancellationToken);
                }
            }
            else
            {
               await _botClient.SendMessage(message.Chat, "Задачи не найдены.", cancellationToken: cancellationToken); // Убираем await
            }
        }

        private async Task CmdReport(Message message, ToDoUser user, CancellationToken cancellationToken)
        {
            var reportService = new ToDoReportService(_toDoRepository);
            var stats = await reportService.GetUserStatsAsync(user.UserId, cancellationToken);
            await _botClient.SendMessage(message.Chat, $"Статистика по задачам на {stats.generatedAt}:\nВсего: {stats.total};\nЗавершенных: {stats.completed};\nАктивных: {stats.active};", cancellationToken: cancellationToken);
        }

        private async Task CmdShowAllTasks(Message message, ToDoUser user,CancellationToken cancellationToken)
        {
            var allTasks = await _toDoService.GetAllByUserIdAsync(user.UserId,cancellationToken);
            if (allTasks.Any())
            {
                foreach (var task in allTasks)
                {
                    await _botClient.SendMessage(message.Chat, $"({task.State}) {task.Name} - {task.CreatedAt} - {task.Id}",cancellationToken: cancellationToken);
                }
            }
            else
            {
               await _botClient.SendMessage(message.Chat, "Задач нет.", cancellationToken: cancellationToken);
            }
        }

        private async Task CmdCompleteTask(Message message, ToDoUser user, string? taskIdStr, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(taskIdStr))
            {
                await _botClient.SendMessage(message.Chat, "Необходимо указать ID задачи.", cancellationToken: cancellationToken);
                return;
            }

            if (!Guid.TryParse(taskIdStr, out var taskId))
            {
                await _botClient.SendMessage(message.Chat, "Некорректный формат ID задачи.", cancellationToken: cancellationToken);
                return;
            }

            await _toDoService.MarkCompletedAsync(taskId, cancellationToken);
            await _botClient.SendMessage(message.Chat, "Задача отмечена как завершённая.", cancellationToken: cancellationToken);

        }

        private async Task CmdRemoveTask(Message message, ToDoUser user, string? taskNumberStr, CancellationToken cancellationToken)
        {
            await CmdShowTasks(message, user,cancellationToken);

            if (string.IsNullOrWhiteSpace(taskNumberStr))
            {
                await _botClient.SendMessage(message.Chat, "Необходимо указать ID задачи.", cancellationToken: cancellationToken);
                return;
            }

            if (!Guid.TryParse(taskNumberStr, out var taskId))
            {
                await _botClient.SendMessage(message.Chat, "Некорректный формат ID задачи.", cancellationToken: cancellationToken);
                return;
            }

            await _toDoService.DeleteAsync(taskId, cancellationToken);
            await _botClient.SendMessage(message.Chat, "Задача успешно удалена.", cancellationToken: cancellationToken);
        }

        private async Task CmdShowTasks(Message message, ToDoUser user, CancellationToken cancellationToken)
        {
            var activeTasks = await _toDoService.GetActiveByUserIdAsync(user.UserId, cancellationToken);
            if (activeTasks.Any())
            {
                int idx = 1; // Номер задачи
                foreach (var task in activeTasks)
                {
                    await _botClient.SendMessage(message.Chat, $"{idx++}. {task.Name} - {task.CreatedAt} - {task.Id}", cancellationToken: cancellationToken);
                }
            }
            else
            {
              await _botClient.SendMessage(message.Chat, "Список задач пуст.", cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Метод для вызова команд, которые доступны не зарегистрированным пользователям.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="message"></param>
        /// <param name="user"></param>
        private async Task ExecutePublicCommand(string cmd, Message message, ToDoUser? user, CancellationToken cancellationToken)
        {
            switch (cmd)
            {
                case "/help":
                   await CmdHelp(message, user, cancellationToken);
                    break;
                case "/info":
                   await CmdInfo(message, user,cancellationToken);
                    break;
            }
        }


        private async Task CmdAddTask(Message message, ToDoUser? user, string? taskName, CancellationToken cancellationToken)

        {
            if (string.IsNullOrWhiteSpace(taskName))
            {
                await _botClient.SendMessage(message.Chat, "Название задачи не может быть пустым.", cancellationToken: cancellationToken);
                return;
            }

            var addedItem = await _toDoService.AddAsync(user, taskName!, cancellationToken);
            await _botClient.SendMessage(message.Chat, $"Задача добавлена: {addedItem.Name} - {addedItem.CreatedAt} - {addedItem.Id}", cancellationToken: cancellationToken);
       }
       

        private async Task CmdInfo(Message message, ToDoUser? user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                await _botClient.SendMessage(message.Chat, "Версия программы: 1.0.\nДата создания: Февраль 2025.", cancellationToken: cancellationToken);
            }
            else
            {
                await _botClient.SendMessage(message.Chat, "Версия программы: 1.0.\nДата создания: Февраль 2025.", cancellationToken: cancellationToken);
            }
        }

        private async Task CmdHelp(Message message, ToDoUser? user, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(message.Chat, @"
            Доступные команды:
            /start - регистрация;
            /help - Показать доступные команды;
            /info - Информация о приложении;
            /addtask имя задачи - Добавить задачу;
            /showtasks - Показать активные задачи;
            /removetask номер-задачи - Удалить задачу;
            /completetask guid-задачи - Завершить задачу;
            /showalltasks - Показать все задачи;
            /report - Показать статистику по задачам;
            /find начало-названия задачи - Найти задачи по названию;
            ", cancellationToken: cancellationToken);
        }
    }
}
