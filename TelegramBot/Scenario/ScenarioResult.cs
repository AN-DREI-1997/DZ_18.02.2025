namespace TelegramBot.Scenarios;

/// <summary>
/// Перечисление enum результата выполнения сценария
/// </summary>
public enum ScenarioResult
{
    /// <summary>
    /// Переход к следующему шагу. Сообщение обработано, но сценарий еще не завершен
    /// </summary>
    Transition, 
    /// <summary>
    /// сценарий завершён
    /// </summary>
    Completed 
}