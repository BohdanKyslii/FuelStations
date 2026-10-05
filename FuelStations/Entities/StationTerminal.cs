namespace FuelStations.Entities;

/// <summary>
/// Workstation identifier (4 digits, e.g., 1030) linked to a single operator cash register.
/// A workstation is assigned multiple identifiers if necessary.
/// </summary>
public class StationTerminal
{
    /// <summary>
    /// Station ID: unique, immutable, assigned manually (1000-9999). Primary key.
    /// </summary>
    public int Number { get; set; }
    
    public int StationId { get; set; }
    
    public Station Station { get; set; } = null!;
    
    /// <summary>
    /// Optional note, e.g. "Cash Desk 1, description of requirements.
    /// </summary>
    public string? Description { get; set; }
}
