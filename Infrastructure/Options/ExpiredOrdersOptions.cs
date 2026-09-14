namespace Infrastructure.Options;

public class ExpiredOrdersOptions
{
    // ✅ Имя секции в appsettings.json
    public const string SectionName = "BackgroundServices:ExpiredOrders";

    /// <summary>
    /// Включён ли сервис очистки
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Через сколько минут неоплаты отменять заказ
    /// </summary> 
    public int PaymentTimeoutMinutes { get; set; } = 30;

    /// <summary>
    /// Как часто проверять просроченные заказы (в минутах)
    /// </summary>
    public int CheckIntervalMinutes { get; set; } = 15;
}