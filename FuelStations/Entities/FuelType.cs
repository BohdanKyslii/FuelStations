namespace FuelStations.Entities;

/// <summary>
/// Запис довідника видів пального (наприклад, A95BIO, DPE), керується через API.
/// Fuel type reference record (e.g. A95BIO, DPE), managed via the API.
/// </summary>
public class FuelType
{
    /// <summary>
    /// Бізнес-код: унікальний, незмінний, у верхньому регістрі (наприклад, "A95BIO").
    /// Первинний ключ.
    /// Business code: unique, immutable, upper case (e.g. "A95BIO"). Primary key.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Назва для користувачів (наприклад, "Бензин А95 біо").
    /// Display name shown to users (e.g. "Бензин А95 біо").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Неактивний вид не можна додати на АЗС, але він лишається в історії.
    /// Inactive fuel types can't be added to stations but stay in history.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Навігаційна властивість: записи пального цього виду на АЗС.
    /// Navigation property: station fuel records of this type.
    /// </summary>
    public List<StationFuel> StationFuels { get; set; } = [];
}
