using System.IO;
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

       public UpdateHandler(ITelegramBotClient botClient, IUserService userService, IToDoService toDoService, IToDoRepository toDoRepository)
        {
            _botClient = botClient;
            _userService = userService;
            _toDoService = toDoService;
            _toDoRepository = toDoRepository;
       }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            try
            {
                if (update.Message != null && update.Message.Text != null)
                {
                    ProcessCommand(update.Message);
                }
            }
            catch (Exception ex)
            {
                _botClient.SendMessage(update.Message.Chat, $"Произошла ошибка: {ex.Message}");
            }
        }

        private void ProcessCommand(Message message)
        {
            if (message.Text.StartsWith('/'))
            {
                var parts = message.Text.Split(' ', 2);
                string cmd = parts.Length > 0 ? parts[0].ToLower() : "";
                string? arg = parts.Length > 1 ? parts[1].Trim() : null;

                // Получаем пользователя
                ToDoUser? user = _userService.GetUser(message.From.Id);

                // Регистрация пользователя при команде /start
                if (cmd == "/start")
                {
                    if (user == null)
                    {
                        user = _userService.RegisterUser(message.From.Id, message.From.Username ?? "");
                        _botClient.SendMessage(message.Chat, $"Привет, {user.TelegramUserName}! Ты успешно зарегистрирован.");
                    }
                    else
                    {
                        _botClient.SendMessage(message.Chat, $"Привет, {user.TelegramUserName}!");
                    }
                    return;
                }
                // Незарегистрированным пользователям доступно только /help и /info
                if (user == null)
                {
                    if (cmd == "/help" || cmd == "/info")
                    {
                        ExecutePublicCommand(cmd, message, user);
                    }
                    else
                    {
                        _botClient.SendMessage(message.Chat, "Вы не зарегистрированы. Для использования программы зарегистрируйтесь командой /start.\n" +
                                                            "Доступны только команды:\n" +
                                                            "/help - показать помощь\n" +
                                                            "/info - получить информацию о приложении");
                    }
                    return;
                }
                // Зарегистрированным пользователям доступны все команды
                switch (cmd)
                {
                    case "/help":
                         CmdHelp(message, user);
                        break;
                    case "/info":
                         CmdInfo(message, user);
                        break;
                    case "/addtask":
                        CmdAddTask(message, user, arg);
                        break;
                    case "/showtasks":
                        CmdShowTasks(message, user);
                        break;
                    case "/removetask":
                        CmdRemoveTask(message, user, arg);
                        break;
                    case "/completetask":
                        CmdCompleteTasl(message, user, arg);
                        break;
                    case "/showalltasks":
                         CmdShowAllTasks(message, user);
                        break;
                    case "/report": // Добавляем обработку команды /report
                         CmdReport(message, user);
                        break;
                    case "/find": // Добавляем обработку команды /find
                         CmdFind(message, user, arg);
                        break;
                    default:
                        _botClient.SendMessage(message.Chat, "Команды не найдено. Используйте /help для помощи.");
                        break;
                }
            }
            else
            {
                // Сообщение не начинается с '/', выводим подсказку
                _botClient.SendMessage(message.Chat, "Команда должна начинаться с символа '/'");
            }
        }

        private void CmdFind(Message message, ToDoUser user, string? arg)
        {
            if (arg == null)
            {
                _botClient.SendMessage(message.Chat, "Введи текст для поиска после команды /find.");
                return;
            }

            var foundTodos = _toDoService.Find(user, arg);
            if (foundTodos.Any())
            {
                foreach (var todo in foundTodos)
                {
                    _botClient.SendMessage(message.Chat, $"{todo.Name} - {todo.CreatedAt} - {todo.Id}");
                }
            }
            else
            {
                _botClient.SendMessage(message.Chat, "Задачи не найдены."); // Убираем await
            }
        }

        private void CmdReport(Message message, ToDoUser user)
        {
            var reportService = new ToDoReportService(_toDoRepository);
            var stats = reportService.GetUserStats(user.UserId);
            _botClient.SendMessage(message.Chat, $"Статистика по задачам на {stats.generatedAt}:\nВсего: {stats.total};\nЗавершенных: {stats.completed};\nАктивных: {stats.active};");
        }

        private void CmdShowAllTasks(Message message, ToDoUser user)
        {
            var allTasks = _toDoService.GetAllByUserId(user.UserId);
            if (allTasks.Any())
            {
                foreach (var task in allTasks)
                {
                    _botClient.SendMessage(message.Chat, $"({task.State}) {task.Name} - {task.CreatedAt} - {task.Id}");
                }
            }
            else
            {
                _botClient.SendMessage(message.Chat, "Задач нет.");
            }
        }

        private void CmdCompleteTasl(Message message, ToDoUser user, string? taskIdStr)
        {
            if (Guid.TryParse(taskIdStr!, out var taskId))
            {
                _toDoService.MarkCompleted(taskId);
                _botClient.SendMessage(message.Chat, "Задача помечена как выполненная.");
            }
            else
            {
                _botClient.SendMessage(message.Chat, "Некорректный идентификатор задачи.");
            }
        }

        private void CmdRemoveTask(Message message, ToDoUser user, string? taskNumberStr)
        {
            CmdShowTasks(message, user);
            if (string.IsNullOrWhiteSpace(taskNumberStr))
            {
                _botClient.SendMessage(message.Chat, "Номер задачи не указан.");
                return;
            }

            if (!int.TryParse(taskNumberStr, out int taskNumber))
            {
                _botClient.SendMessage(message.Chat, "Некорректный номер задачи.");
                return;
            }

            // Получаем активный список задач
            var activeTasks = _toDoService.GetActiveByUserId(user.UserId);
            if (taskNumber > 0 && taskNumber <= activeTasks.Count)
            {
                var taskToDelete = activeTasks.ElementAt(taskNumber - 1);
                _toDoService.Delete(taskToDelete.Id);
                _botClient.SendMessage(message.Chat, "Задача успешно удалена.");
            }
            else
            {
                _botClient.SendMessage(message.Chat, "Некорректный номер задачи.");
            }
        }

        private void CmdShowTasks(Message message, ToDoUser user)
        {
            var activeTasks = _toDoService.GetActiveByUserId(user.UserId);
            if (activeTasks.Any())
            {
                int idx = 1; // Номер задачи
                foreach (var task in activeTasks)
                {
                    _botClient.SendMessage(message.Chat, $"{idx++}. {task.Name} - {task.CreatedAt} - {task.Id}");
                }
            }
            else
            {
                _botClient.SendMessage(message.Chat, "Список задач пуст.");
            }
        }

        /// <summary>
        /// Метод для вызова команд, которые доступны не зарегистрированным пользователям.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="message"></param>
        /// <param name="user"></param>
        private void ExecutePublicCommand(string cmd, Message message, ToDoUser? user)
        {
            switch (cmd)
            {
                case "/help":
                    CmdHelp(message, user);
                    break;
                case "/info":
                    CmdInfo(message, user);
                    break;
            }
        }

        private void CmdAddTask(Message message, ToDoUser? user, string? taskName)
        {
            if (string.IsNullOrWhiteSpace(taskName))
            {
                _botClient.SendMessage(message.Chat, "Название задачи не может быть пустым.");
                return;
            }
            var addedItem = _toDoService.Add(user, taskName!);
            _botClient.SendMessage(message.Chat, $"Задача добавлена: {addedItem.Name} - {addedItem.CreatedAt} - {addedItem.Id}");
        }

        private void CmdInfo(Message message, ToDoUser? user)
        {
            if (user == null)
            {
                _botClient.SendMessage(message.Chat, "Версия программы: 1.0.\nДата создания: Февраль 2025.");
            }
            else
            {
                _botClient.SendMessage(message.Chat, "Версия программы: 1.0.\nДата создания: Февраль 2025.");
            }
        }

        private void CmdHelp(Message message, ToDoUser? user)
        {
            _botClient.SendMessage(message.Chat, @"
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
            ");
        }
    }
}
