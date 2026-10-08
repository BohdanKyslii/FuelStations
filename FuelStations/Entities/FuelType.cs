namespace FuelStations.Entities;

/// <summary>
/// Запис довідника видів пального (наприклад, A95BIO, DPE), керується через API.
/// </summary>
public class FuelType
{
    /// <summary>
    /// Бізнес-код: унікальний, незмінний, у верхньому регістрі (наприклад, "A95BIO").
    /// Первинний ключ.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Назва для користувачів (наприклад, "Бензин А95 біо").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Неактивний вид не можна додати на АЗС, але він лишається в історії.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Навігаційна властивість: записи пального цього виду на АЗС.
    /// </summary>
    public List<StationFuel> StationFuels { get; set; } = [];
}
