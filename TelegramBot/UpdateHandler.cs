using System.IO;
using System.Threading.Tasks;
using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Entities;
using DZ_18._02._2025.Core.Services;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using static DZ_18._02._2025.Core.Exceptions.MyCustomException;

namespace DZ_18._02._2025.TelegramBot
{
    internal class UpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        private readonly IToDoRepository _toDoRepository; // Добавляем репозиторий задач

        public int _maxTasklength { get; private set; }
        public int _maxTaskCount { get; private set; }

        // Делегаты и события
        public delegate void MessageEventHandler(string message);
        public event MessageEventHandler? OnHandleUpdateStarted;
        public event MessageEventHandler? OnHandleUpdateCompleted;

        public UpdateHandler(ITelegramBotClient botClient, IUserService userService, IToDoService toDoService, IToDoRepository toDoRepository, int maxTaskCount, int maxTasklength)
        {
            _botClient = botClient;
            _userService = userService;
            _toDoService = toDoService;
            _toDoRepository = toDoRepository;
            _maxTasklength = maxTasklength;
            _maxTaskCount = maxTaskCount;
        }
       
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message != null && update.Message.Text != null)
                {
                    OnHandleUpdateStarted?.Invoke(update.Message.Text);
                    await ProcessCommand(update.Message, cancellationToken);
                    OnHandleUpdateCompleted?.Invoke(update.Message.Text);
                }
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(botClient, ex, cancellationToken);
            }
        }

        // Реализация метода HandleErrorAsync
        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            await botClient.SendMessage(new Chat { Id = 1 }, "Возникла ошибка: " + exception.Message, cancellationToken);
        }

        private async Task ProcessCommand(Message message, CancellationToken cancellationToken)
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
                       await _botClient.SendMessage(message.Chat, $"Привет, {user.TelegramUserName}! Ты успешно зарегистрирован.", cancellationToken);
                    }
                    else
                    {
                       await _botClient.SendMessage(message.Chat, $"Привет, {user.TelegramUserName}!", cancellationToken);
                    }
                    return;
                }
                // Незарегистрированным пользователям доступно только /help и /info
                if (user == null)
                {
                    if (cmd == "/help" || cmd == "/info")
                    {
                        ExecutePublicCommand(cmd, message, user,cancellationToken);
                    }
                    else
                    {
                       await _botClient.SendMessage(message.Chat, "Вы не зарегистрированы. Для использования программы зарегистрируйтесь командой /start.\n" +
                                                            "Доступны только команды:\n" +
                                                            "/help - показать помощь\n" +
                                                            "/info - получить информацию о приложении", cancellationToken);
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
                        await CmdAddTask(message, user, arg, _maxTaskCount,_maxTasklength, cancellationToken);
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
                        await  _botClient.SendMessage(message.Chat, "Команды не найдено. Используйте /help для помощи.", cancellationToken);
                        break;
                }
            }
            else
            {
                // Сообщение не начинается с '/', выводим подсказку
               await _botClient.SendMessage(message.Chat, "Команда должна начинаться с символа '/'", cancellationToken);
            }
        }

        private async Task CmdFind(Message message, ToDoUser user, string? arg, CancellationToken cancellationToken)
        {
            if (arg == null)
            {
                await _botClient.SendMessage(message.Chat, "Введи текст для поиска после команды /find.", cancellationToken);
                return;
            }

            var foundTodos = await _toDoService.FindAsync(user, arg,cancellationToken);
            if (foundTodos.Any())
            {
                foreach (var todo in foundTodos)
                {
                    await _botClient.SendMessage(message.Chat, $"{todo.Name} - {todo.CreatedAt} - {todo.Id}", cancellationToken);
                }
            }
            else
            {
               await _botClient.SendMessage(message.Chat, "Задачи не найдены.", cancellationToken); // Убираем await
            }
        }

        private async Task CmdReport(Message message, ToDoUser user, CancellationToken cancellationToken)
        {
            var reportService = new ToDoReportService(_toDoRepository);
            var stats = await reportService.GetUserStatsAsync(user.UserId, cancellationToken);
            await _botClient.SendMessage(message.Chat, $"Статистика по задачам на {stats.generatedAt}:\nВсего: {stats.total};\nЗавершенных: {stats.completed};\nАктивных: {stats.active};", cancellationToken);
        }

        private async Task CmdShowAllTasks(Message message, ToDoUser user,CancellationToken cancellationToken)
        {
            var allTasks = await _toDoService.GetAllByUserIdAsync(user.UserId,cancellationToken);
            if (allTasks.Any())
            {
                foreach (var task in allTasks)
                {
                    await _botClient.SendMessage(message.Chat, $"({task.State}) {task.Name} - {task.CreatedAt} - {task.Id}",cancellationToken);
                }
            }
            else
            {
               await _botClient.SendMessage(message.Chat, "Задач нет.", cancellationToken);
            }
        }

        private async Task CmdCompleteTask(Message message, ToDoUser user, string? taskIdStr, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(taskIdStr))
            {
                await _botClient.SendMessage(message.Chat, "Необходимо указать ID задачи.", cancellationToken);
                return;
            }

            if (!Guid.TryParse(taskIdStr, out var taskId))
            {
                await _botClient.SendMessage(message.Chat, "Некорректный формат ID задачи.", cancellationToken);
                return;
            }

            await _toDoService.MarkCompletedAsync(taskId, cancellationToken);
            await _botClient.SendMessage(message.Chat, "Задача отмечена как завершённая.", cancellationToken);

        }

        private async Task CmdRemoveTask(Message message, ToDoUser user, string? taskNumberStr, CancellationToken cancellationToken)
        {
            await CmdShowTasks(message, user,cancellationToken);

            if (string.IsNullOrWhiteSpace(taskNumberStr))
            {
                await _botClient.SendMessage(message.Chat, "Необходимо указать ID задачи.", cancellationToken);
                return;
            }

            if (!Guid.TryParse(taskNumberStr, out var taskId))
            {
                await _botClient.SendMessage(message.Chat, "Некорректный формат ID задачи.", cancellationToken);
                return;
            }

            await _toDoService.DeleteAsync(taskId, cancellationToken);
            await _botClient.SendMessage(message.Chat, "Задача успешно удалена.", cancellationToken);
        }

        private async Task CmdShowTasks(Message message, ToDoUser user, CancellationToken cancellationToken)
        {
            var activeTasks = await _toDoService.GetActiveByUserIdAsync(user.UserId, cancellationToken);
            if (activeTasks.Any())
            {
                int idx = 1; // Номер задачи
                foreach (var task in activeTasks)
                {
                    await _botClient.SendMessage(message.Chat, $"{idx++}. {task.Name} - {task.CreatedAt} - {task.Id}", cancellationToken);
                }
            }
            else
            {
              await _botClient.SendMessage(message.Chat, "Список задач пуст.", cancellationToken);
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

        private async Task CmdAddTask(Message message, ToDoUser? user, string? taskName, int maxTaskCount, int maxTasklength, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(taskName))
            {
                await _botClient.SendMessage(message.Chat, "Название задачи не может быть пустым.", cancellationToken);
                return;
            }
            var addedItem = await _toDoService.AddAsync(user, taskName!, maxTaskCount, maxTasklength, cancellationToken);
            await _botClient.SendMessage(message.Chat, $"Задача добавлена: {addedItem.Name} - {addedItem.CreatedAt} - {addedItem.Id}",cancellationToken);
        }

        private async Task CmdInfo(Message message, ToDoUser? user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                await _botClient.SendMessage(message.Chat, "Версия программы: 1.0.\nДата создания: Февраль 2025.", cancellationToken);
            }
            else
            {
                await _botClient.SendMessage(message.Chat, "Версия программы: 1.0.\nДата создания: Февраль 2025.",cancellationToken);
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
            ", cancellationToken);
        }
    }
}