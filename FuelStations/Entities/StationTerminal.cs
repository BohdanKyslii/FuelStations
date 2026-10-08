namespace FuelStations.Entities;

/// <summary>
/// ID АЗС (4 цифри, наприклад 1030), закріплений за одним вікном каси оператора.
/// За потреби АЗС отримує кілька ID.
/// Station ID (4 digits, e.g. 1030) bound to one operator cashier window.
/// A station gets several IDs if needed.
/// </summary>
public class StationTerminal
{
    /// <summary>
    /// ID АЗС: унікальний, незмінний, задається вручну (1000–9999). Первинний ключ.
    /// Station ID: unique, immutable, assigned manually (1000-9999). Primary key.
    /// </summary>
    public int Number { get; set; }

    public int StationId { get; set; }

    public Station Station { get; set; } = null!;

    /// <summary>
    /// Необов'язковий опис, наприклад "Каса 1, колонки 1–7".
    /// Optional note, e.g. "Каса 1, колонки 1–7".
    /// </summary>
    public string? Description { get; set; }
}
