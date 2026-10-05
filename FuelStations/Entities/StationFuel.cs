namespace FuelStations.Entities;

/// <summary>
/// Fuel offered at a station, with its current price.
/// </summary>
public class StationFuel
{
    public int Id { get; set; }

    public int StationId { get; set; }

    public Station Station { get; set; } = null!;

    public FuelType FuelType { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime PriceUpdatedAt { get; set; } = DateTime.UtcNow;
}
