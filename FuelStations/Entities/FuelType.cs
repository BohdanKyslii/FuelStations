namespace FuelStations.Entities;

/// <summary>
/// Fuel type reference record (e.g. A95, DIESEL), managed via the API.
/// </summary>
public class FuelType
{
    /// <summary>
    /// Business code: unique, immutable, upper case (e.g. "A95"). Primary key.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name shown to users (e.g. "А-95").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Inactive fuel types can't be added to stations but stay in history.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property: station fuel records of this type.
    /// </summary>
    public List<StationFuel> StationFuels { get; set; } = [];
}
