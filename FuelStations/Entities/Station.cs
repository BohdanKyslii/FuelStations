namespace FuelStations.Entities;

/// <summary>
/// АЗС (фізична заправка). В API адресується будь-яким зі своїх ID.
/// Gas station (physical site). Addressed in the API by any of its station IDs.
/// </summary>
public class Station
{
    /// <summary>
    /// Технічний ключ БД. Лише для зв'язків між таблицями, в API не виходить.
    /// Technical database key. Used only for table relations, never exposed in the API.
    /// </summary>
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// true — АЗС працює, false — тимчасово закрита.
    /// True if the station is operating, false if temporarily closed.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Бізнес-ID АЗС (по одному на вікно каси оператора).
    /// Business station IDs (one per cashier window).
    /// </summary>
    public List<StationTerminal> Terminals { get; set; } = [];

    /// <summary>
    /// Пальне, що продається на цій АЗС, з поточними цінами.
    /// Fuels sold at this station with current prices.
    /// </summary>
    public List<StationFuel> Fuels { get; set; } = [];
}
