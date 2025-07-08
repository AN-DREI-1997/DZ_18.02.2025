using DZ_18._02._2025.Services;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace DZ_18._02._2025
{
    internal class UpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;

        public UpdateHandler(IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoService = toDoService;
        }

        public UpdateHandler(ITelegramBotClient botClient, IUserService userService, IToDoService toDoService)
        {
            _botClient = botClient;
            _userService = userService;
            _toDoService = toDoService;
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            //botClient.SendMessage(update.Message.Chat, $"Получил '{update.Message.Text}'");
            try
            {
                var message = update.Message;
                var userId = message.From.Id;
                var userName = message.From.Username;

                var user = _userService.GetUser(userId);
                var chat = new Chat { Id = update.Message.Chat.Id };
                if (message.Text.StartsWith("/start"))
                {
                    var registeredUser = _userService.RegisterUser(userId, userName);
                    _botClient.SendMessage(chat, "Вы успешно зарегистрированы.");
                    return;
                }

                if (user == null)
                {
                    _botClient.SendMessage(chat, "Вы не зарегистрированы. Используйте /start.");
                    return;
                }

                var text = message.Text;

                if (text.StartsWith("/help"))
                {
                    _botClient.SendMessage(chat,
                       "/start - регистрация\n" +
                       "/addtask [имя задачи]\n" +
                       "/removetask [ID]\n" +
                       "/completetask [ID]\n" +
                       "/showtasks\n" +
                       "/showalltasks\n" +
                       "/help");
                }
                else if (text.StartsWith("/addtask"))
                {
                    var name = text[9..];
                    var item = _toDoService.Add(user, name);
                    _botClient.SendMessage(chat, $"Задача добавлена: {item.Name}");
                }
                else if (text.StartsWith("/removetask"))
                {
                    var id = Guid.Parse(text[12..]);
                    _toDoService.Delete(id);
                    _botClient.SendMessage(chat, "Задача удалена.");
                }
                else if (text.StartsWith("/completetask"))
                {
                    var id = Guid.Parse(text[15..]);
                    _toDoService.MarkCompleted(id);
                    _botClient.SendMessage(chat, "Задача отмечена как выполненная.");
                }
                else if (text.StartsWith("/showtasks"))
                {
                    var tasks = _toDoService.GetActiveByUserId(user.UserId);
                    var lines = tasks.Select(t => $"{t.Name} - {t.CreatedAt:G} - {t.Id}");
                    _botClient.SendMessage(chat, string.Join("\n", lines));
                }
                else if (text.StartsWith("/showalltasks"))
                {
                    var tasks = _toDoService.GetAllByUserId(user.UserId);
                    var lines = tasks.Select(t => $"({t.State}) {t.Name} - {t.CreatedAt:G} - {t.Id}");
                    _botClient.SendMessage(chat, string.Join("\n", lines));
                }
                else if (text.StartsWith("/info"))
                {
                    _botClient.SendMessage(chat, "Версия программы: 1.0.\nДата создания: Февраль 2025.");
                }
                else
                {
                    _botClient.SendMessage(chat, "Неизвестная команда. Используйте /help.");
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}