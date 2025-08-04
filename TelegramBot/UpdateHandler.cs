using DZ_18._02._2025.Core.DadaAccess;
using DZ_18._02._2025.Core.Entities;
using DZ_18._02._2025.Core.Services;
using DZ_18._02._2025.TelegramBot.Scenario;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Scenarios;
using static DZ_18._02._2025.Core.Exceptions.MyCustomException;

namespace DZ_18._02._2025.TelegramBot
{
    internal class UpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        private readonly IToDoRepository _toDoRepository;
        private readonly IScenario[] _scenarios;
        private readonly IScenarioContextRepository _contextRepository;

        //список доступных команд
        private readonly List<string> _registredUserCommands = new List<string>()
        { "/addtask", "/showtasks", "/removetask", "/completetask", "/showalltasks", "/cancel", "/exit", "/start", "/report", "/find" };
        /// <summary>
        /// Левая граница диапазона значений для максимально количества задач.
        /// </summary>
        const int MinCountLimit = 0;

        /// <summary>
        /// Правая граница диапазона значений для максимально количества задач.
        /// </summary>
        const int MaxCountLimit = 100;

        /// <summary>
        /// Левая граница диапазона допустимой длины задач.
        /// </summary>
        const int MinLengthLimit = 1;

        /// <summary>
        /// Правая граница диапазона допустимой длины задач.
        /// </summary>
        const int MaxLengthLimit = 100;

        // Делегаты и события
        public delegate void MessageEventHandler(string message);
        public event MessageEventHandler? UpdateStarted;
        public event MessageEventHandler? UpdateCompleted;

        public UpdateHandler(
            ITelegramBotClient botClient,
            IUserService userService,
            IToDoService toDoService,
            IToDoRepository toDoRepository,
            IScenario[] scenarios,
            IScenarioContextRepository contextRepository)
        {
            _botClient = botClient;
            _userService = userService;
            _toDoService = toDoService;
            _toDoRepository = toDoRepository;
            _scenarios = scenarios;
            _contextRepository = contextRepository;
        }
        /// <summary>
        /// метод IScenario GetScenario(ScenarioType scenario), который возвращает соответствующий сценарий. Если сценарий не найден, то выбрасывать исключение
        /// </summary>
        /// <param name="scenario"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        IScenario GetScenario(ScenarioType scenario)
        {
            var result = _scenarios.FirstOrDefault(s => s.CanHandle(scenario));

            if (result == null)
            {
                throw new KeyNotFoundException($"Scenario {scenario} not found");
            }

            return result;
        }
        public void OnHandleUpdateStarted(string message)
        {
            Console.WriteLine($"Началась обработка сообщения '{message}'");
        }
        public void OnHandleUpdateCompleted(string message)
        {
            Console.WriteLine($"Закончилась обработка сообщения '{message}'");
        }

        private async Task ProcessScenario(ITelegramBotClient botClient, ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenario = GetScenario(context.CurrentScenario);

            var scenarioResult = await scenario.HandleMessageAsync(botClient, context, update, ct);

            var userId = update.Message?.From?.Id ?? update.CallbackQuery?.From?.Id ?? 0;

            switch (scenarioResult)
            {
                case ScenarioResult.Completed:
                    await _contextRepository.ResetContext(userId, ct);
                    break;
                case ScenarioResult.Transition:
                    await _contextRepository.SetContext(userId, context, ct);
                    break;
            }
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            string InfoMessage = "Доступны команды: start, help, info, addtask, showtasks, removetask, completetask, showalltasks, cancel, report, find, exit. При вводе команды указываейте вначале символ / (слеш).";
            var commands = new List<BotCommand>
            {
                new BotCommand {Command = "start", Description = "Старт бота"},
                new BotCommand {Command = "help", Description = "Подсказка по командам бота"},
                new BotCommand {Command = "info", Description = "Информация по версии и дате версии бота"},
                new BotCommand {Command = "addtask", Description = "Добавление новой задачи"},
                new BotCommand {Command = "showtasks", Description = "Отображение списка задач"},
                new BotCommand {Command = "removetask", Description = "Удаление задачи"},
                new BotCommand {Command = "completetask", Description = "Завершение задачи"},
                new BotCommand {Command = "showalltasks", Description = "Отображение списка задач со статусами"},
                new BotCommand {Command = "cancel", Description = "Отмена выполнения сценария"},
                new BotCommand {Command = "report", Description = "Статистика по задачам"},
                new BotCommand {Command = "find", Description = "Поиск задачи"},
                new BotCommand {Command = "exit", Description = "Завершение работы с ботом"}
            };
            if (update.Message is not { } message)
                return;

            var userId = message.From.Id;
            ToDoUser? user = await _userService.GetUserAsync(message.From.Id, ct);
            // Проверяем наличие активного сценария
            var context = await _contextRepository.GetContext(userId, ct);
            string userCommand = update.Message.Text;
            if (userCommand == "/cancel")
            {
                await _contextRepository.ResetContext(update?.Message?.From?.Id ?? 0, ct);
                await botClient.SendMessage(update.Message.Chat, "Текущий сценарий отменен", replyMarkup: KeyboardHelper.GetDefaultKeyboard(), cancellationToken: ct);
                return;
            }

            if (context != null && !string.IsNullOrEmpty(context.CurrentStep))
            {
                await ProcessScenario(botClient, context, update, ct);
                return;
            }
            await botClient.SetMyCommands(commands);
            
            var toDoUser = await _userService.GetUserAsync(update.Message.From.Id, ct);
            ReplyKeyboardMarkup replyMarkup;
            if (toDoUser == null)
            {
                if (userCommand != "/start")
                {
                    replyMarkup = new ReplyKeyboardMarkup(
                        new[] { new KeyboardButton[] { "/start" } });
                    await botClient.SendMessage(
                        update.Message.Chat,
                        "Зарегистрируйтесь, выполнив команду start",
                        cancellationToken: ct,
                        replyMarkup: replyMarkup);
                }
                else
                {
                    replyMarkup = new ReplyKeyboardMarkup();
                }
            }
            else
            {
                replyMarkup = KeyboardHelper.GetDefaultKeyboard();
            }
            replyMarkup.ResizeKeyboard = true; //авторазмер кнопок в самом TG
            UpdateStarted.Invoke(userCommand);

            switch (userCommand)
            {
                case "/help":
                    await CmdHelp(botClient, update, ct, replyMarkup);
                    break;
                case "/info":
                    await CmdInfo(botClient,update, ct, replyMarkup);
                    break;
                case "/start":
                    await Start(botClient, update, ct);
                    break;
                default:
                    var indx = userCommand.IndexOf(" ");
                    if (_registredUserCommands.Contains(userCommand[..(indx == -1 ? userCommand.Length : indx)]
                            .Trim()))
                    {
                        if (toDoUser != null)
                        {
                            switch (userCommand)
                            {
                                case "/addtask":
                                    await CmdAddTaskAsync(botClient,update, ct);
                                    break;
                                case "/showtasks":
                                    await CmdShowTasks(botClient, update, ct, replyMarkup);
                                    break;
                                case string remStr when remStr.StartsWith("/removetask "):
                                    await CmdRemoveTask(botClient, update, userCommand.Substring("/removetask ".Length), ct, replyMarkup);
                                    break;
                                case string bc when bc.StartsWith("/completetask "):
                                    CmdCompleteTask(message,userCommand.Substring("/completetask ".Length), ct);
                                    break;
                                case "/showalltasks":
                                    await CmdShowAllTasks(botClient, update, ct, replyMarkup);
                                    break;
                                case "/report":
                                    await CmdReport(botClient, update, ct, replyMarkup);
                                    break;
                                case "/cancel":
                                    await CancelAsync(botClient, update, ct);
                                    break;
                                case string bc when bc.StartsWith("/find "):
                                    await CmdFind(botClient, update, userCommand.Substring("/find ".Length), ct, replyMarkup);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        await NonCommandAsync(botClient, update, userCommand, InfoMessage, ct, replyMarkup);
                    }

                    break;
            }
            UpdateCompleted.Invoke(userCommand);
        }

        private async Task CancelAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            var context = await _contextRepository.GetContext(update?.Message?.From?.Id ?? 0, ct);

            if (context != null && !string.IsNullOrEmpty(context.CurrentStep))
            {
                await _contextRepository.ResetContext(update?.Message?.From?.Id ?? 0, ct);

                await botClient.SendMessage(
                    update.Message.Chat,
                    "Отмена выполнения сценария",
                    cancellationToken: ct,
                    replyMarkup: KeyboardHelper.GetDefaultKeyboard());
            }
        }


        // Метод для экранирования спецсимволов Markdown
        private string EscapeMarkdownCharacters(string input)
        {
            return input
                .Replace("\\", "\\\\")          // Экранируем сам символ \
                .Replace("-", "\\-");           // Экранируем дефис -
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка при обработке обновления: {exception.Message}");

            if (exception is Telegram.Bot.Exceptions.ApiRequestException apiEx && !string.IsNullOrEmpty(apiEx.Message))
            {
                await botClient.SendMessage(-1 /* Здесь укажите реальный Chat ID */, "Возникла внутренняя ошибка. Попробуйте позже.", cancellationToken: cancellationToken);
            }
            else
            {
                Console.WriteLine("Сообщение об ошибке невозможно передать пользователю.");
            }
        }

        /// <summary>
        /// Метод для регистрации пользователя
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task Start(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var from = update?.Message?.From;
            var toDoUser = await _userService.RegisterUserAsync(from?.Id ?? 0, from?.Username, cancellationToken);
            if (toDoUser != null)
            {
                await botClient.SendMessage(
                    update.Message.Chat,
                    "Для отображения информации по задачам можно воспользоваться кнопками",
                    cancellationToken: cancellationToken,
                    replyMarkup: KeyboardHelper.GetDefaultKeyboard());
            }
        }
        private async Task CmdFind(ITelegramBotClient botClient, Update update, string? arg, CancellationToken cancellationToken, ReplyKeyboardMarkup replyKeyboard)
        {
            if (arg == null)
            {
                await _botClient.SendMessage(update.Message.Chat, "Введи текст для поиска после команды /find.", cancellationToken: cancellationToken);
                return;
            }

            var foundTodos = await _toDoService.FindAsync(Guid.Parse(update.Message.From.Id.ToString()), arg, cancellationToken);
            if (foundTodos.Any())
            {
                foreach (var todo in foundTodos)
                {
                    await _botClient.SendMessage(update.Message.Chat, $"{todo.Name} - {todo.CreatedAt} - {todo.Id}", cancellationToken: cancellationToken, replyMarkup:replyKeyboard);
                }
            }
            else
            {
                await _botClient.SendMessage(update.Message.Chat, "Задачи не найдены.", cancellationToken: cancellationToken);
            }
        }

        private async Task CmdReport(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, ReplyKeyboardMarkup replyKeyboard)
        {
            var reportService = new ToDoReportService(_toDoRepository);
            var stats = await reportService.GetUserStatsAsync(Guid.Parse(update.Message.From.Id.ToString()), cancellationToken);
            await _botClient.SendMessage(update.Message.Chat, $"Статистика по задачам на {stats.generatedAt}:" +
                $"\nВсего: {stats.total};" +
                $"\nЗавершенных: {stats.completed};" +
                $"\nАктивных: {stats.active};", cancellationToken: cancellationToken, replyMarkup:replyKeyboard);
        }

        private async Task CmdShowAllTasks(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, ReplyKeyboardMarkup replyKeyboard)
        {
            var toDoUser = await _userService.GetUserAsync(update.Message.From.Id, cancellationToken);
            var usersId = toDoUser?.UserId ?? Guid.Empty;
            if (usersId == null)
            {
                await _botClient.SendMessage(update.Message.Chat, "Пользователь не зарегистрирован, список задач недоступен!", cancellationToken: cancellationToken);
            }

            if((await _toDoService.GetAllByUserIdAsync(usersId, cancellationToken)).Count == 0)
            {
                    await _botClient.SendMessage(update.Message.Chat, "Задач нет.", cancellationToken: cancellationToken);
            }
            else
            {
                foreach (var task in await _toDoService.GetAllByUserIdAsync(usersId, cancellationToken))
                {
                    await botClient.SendMessage(update.Message.Chat,
                        Regex.Replace($"({Enum.GetName(task.State)}) {task.Name} - {task.CreatedAt} - `{task.Id}`", "[-\\.\\(\\)\\[\\]\\+\\!\\=_\\|\\*\\~\\>\\#\\{\\}]", "\\$0"),
                        cancellationToken: cancellationToken,
                        parseMode: ParseMode.MarkdownV2,
                        replyMarkup: replyKeyboard);
                }
            }
        }

        void CmdCompleteTask(Message message, string? taskIdStr, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(taskIdStr))
            {
                 _botClient.SendMessage(message.Chat, "Необходимо указать ID задачи.", cancellationToken: cancellationToken);
                return;
            }

            if (!Guid.TryParse(taskIdStr, out var taskId))
            {
                 _botClient.SendMessage(message.Chat, "Некорректный формат ID задачи.", cancellationToken: cancellationToken);
                return;
            }

             _toDoService.MarkCompletedAsync(taskId, cancellationToken);
             _botClient.SendMessage(message.Chat, "Задача отмечена как завершённая.", cancellationToken: cancellationToken);
        }

        private async Task CmdRemoveTask(ITelegramBotClient botClient, Update update, string? taskNumberStr, CancellationToken cancellationToken, ReplyKeyboardMarkup replyKeyboard)
        {
            await CmdShowTasks(botClient, update, cancellationToken, replyKeyboard);

            if (string.IsNullOrWhiteSpace(taskNumberStr))
            {
                await _botClient.SendMessage(update.Message.Chat, "Необходимо указать ID задачи.", cancellationToken: cancellationToken);
                return;
            }

            if (!Guid.TryParse(taskNumberStr, out var taskId))
            {
                await _botClient.SendMessage(update.Message.Chat, "Некорректный формат ID задачи.", cancellationToken: cancellationToken);
                return;
            }

            await _toDoService.DeleteAsync(taskId, cancellationToken);
            await _botClient.SendMessage(update.Message.Chat, "Задача успешно удалена.", cancellationToken: cancellationToken);
        }

        private async Task CmdShowTasks(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, ReplyKeyboardMarkup replyKeyboard)
        {
            var toDoUser = await _userService.GetUserAsync(update.Message.From.Id, cancellationToken);
            var usersId = toDoUser?.UserId ?? Guid.Empty;        
            if (usersId == null)
            {
                await _botClient.SendMessage(update.Message.Chat, "Пользователь не зарегистрирован, список задач недоступен!", cancellationToken: cancellationToken);
            }

            if ((await _toDoService.GetAllByUserIdAsync(usersId, cancellationToken)).Count == 0)
            {
                await _botClient.SendMessage(update.Message.Chat, "Список задач пуст.", cancellationToken: cancellationToken);
            }
            else
            {
                foreach (var task in (await _toDoService.GetActiveByUserIdAsync(usersId, cancellationToken)))
                {
                    await botClient.SendMessage(update.Message.Chat,
                                            Regex.Replace($"{task.Name} - {task.CreatedAt} - `{task.Id}`", "[-\\.\\(\\)\\[\\]\\+\\!\\=_\\|\\*\\~\\>\\#\\{\\}]", "\\$0"),
                                                cancellationToken: cancellationToken,
                                                parseMode: ParseMode.MarkdownV2,
                                                replyMarkup: replyKeyboard);
                }
            }
        }

        async Task CmdAddTaskAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            var toDoUser = await _userService.GetUserAsync(update.Message.From.Id, ct);

            ScenarioContext context = new ScenarioContext(toDoUser.TelegramUserId, ScenarioType.AddTask);

            await _contextRepository.SetContext(update.Message.From.Id, context, ct);

            await ProcessScenario(botClient, context, update, ct);

        }
        
        private async Task CmdInfo(ITelegramBotClient botClient, Update update, CancellationToken ct, ReplyKeyboardMarkup replyKeyboard)
        {
           await _botClient.SendMessage(update.Message.Chat, "Версия программы: 1.0.\nДата создания: Февраль 2025.", cancellationToken: ct, replyMarkup:replyKeyboard);
        }

        private async Task CmdHelp(ITelegramBotClient botClient, Update update, CancellationToken ct, ReplyKeyboardMarkup replyKeyboard)
        {
            await _botClient.SendMessage(update.Message.Chat, @"
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
            ", cancellationToken: ct, replyMarkup: replyKeyboard);
        }
        async Task NonCommandAsync(ITelegramBotClient botClient, Update update, string str, string infoMessage, CancellationToken ct, ReplyKeyboardMarkup replyMarkup)
        {
            if (!string.IsNullOrEmpty(str))
            {
                await botClient.SendMessage(update.Message.Chat, await ReplayAsync(update, $"Команда {str} не предусмотрена к обработке.\n" + infoMessage, ct), cancellationToken: ct, replyMarkup: replyMarkup);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="update"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task<string> ReplayAsync(Update update, string message, CancellationToken ct)
        {
            var toDoUser = await _userService.GetUserAsync(update.Message.From.Id, ct);
            if (toDoUser == null || string.IsNullOrEmpty(toDoUser.TelegramUserName))
            {
                return message;
            }
            return $"{toDoUser.TelegramUserName}, " + message?.First().ToString().ToLower() + message?.Substring(1);
        }
    }
}