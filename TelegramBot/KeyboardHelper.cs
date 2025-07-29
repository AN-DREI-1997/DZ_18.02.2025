using Telegram.Bot.Types.ReplyMarkups;

public class KeyboardHelper
{
    public static InlineKeyboardMarkup CreateStartButton()
    {
        return new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("/start", "/start"),
        });
    }

    //public static InlineKeyboardMarkup CreateRegisteredButtons()
    //{
    //    return new InlineKeyboardMarkup(new[]
    //    {
    //        InlineKeyboardButton.WithCallbackData("/start", "/start")
    //    });
    //}

    public static ReplyKeyboardMarkup CreateRegisteredButtons()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new[] {new KeyboardButton("/showalltasks")},
            new[] {new KeyboardButton("/showtasks")},
            new[] {new KeyboardButton("/report")},
            new[] {new KeyboardButton( "/cancel" )}
        })
        {
            ResizeKeyboard = true
        };
    }
    public static ReplyKeyboardMarkup CanceldButtons()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new[] {new KeyboardButton( "/cancel" )}
        })
        {
            ResizeKeyboard = true
        };
    }
}