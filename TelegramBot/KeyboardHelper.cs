using Telegram.Bot.Types.ReplyMarkups;

public class KeyboardHelper
{
    public static ReplyKeyboardMarkup GetDefaultKeyboard()
    {

        return new ReplyKeyboardMarkup(
        [
            new KeyboardButton[] { "/addtask", "/showalltasks", "/showtasks", "/report" }
        ])

       {
            ResizeKeyboard = true
        };
    }
}