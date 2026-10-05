namespace FuelStations.Entities;

/// <summary>
/// Gas station (physical site). Addressed in the API by any of its station IDs.
/// </summary>
public class Station
{
    /// <summary>
    /// Technical database key. Used only for table relations, never exposed in the API.
    /// </summary>
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
    
    public string? Descripton { get; set; } = string.Empty;

    /// <summary>
    /// True if the station is operating, false if temporarily closed.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Business station IDs (one per cashier window).
    /// </summary>
    public List<StationTerminal> Terminals { get; set; } = [];

    /// <summary>
    /// Fuels sold at this station with current prices.
    /// </summary>
    public List<StationFuel> Fuels { get; set; } = [];
}
