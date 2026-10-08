namespace FuelStations.Entities;

/// <summary>
/// АЗС (фізична заправка). В API адресується будь-яким зі своїх ID.
/// </summary>
public class Station
{
    /// <summary>
    /// Технічний ключ БД. Лише для зв'язків між таблицями, в API не виходить.
    /// </summary>
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// true — АЗС працює, false — тимчасово закрита.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Бізнес-ID АЗС (по одному на вікно каси оператора).
    /// </summary>
    public List<StationTerminal> Terminals { get; set; } = [];

    /// <summary>
    /// Пальне, що продається на цій АЗС, з поточними цінами.
    /// </summary>
    public List<StationFuel> Fuels { get; set; } = [];
}
