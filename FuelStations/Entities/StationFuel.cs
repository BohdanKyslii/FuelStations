namespace FuelStations.Entities;

/// <summary>
/// Fuel offered at a station, with its current price.
/// </summary>
public class StationFuel
{
    public int Id { get; set; }

    public int StationId { get; set; }

    public Station Station { get; set; } = null!;

    /// <summary>
    /// Foreign key to <see cref="FuelType.Code"/>.
    /// </summary>
    public string FuelTypeCode { get; set; } = string.Empty;

    public FuelType FuelType { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsAvailable { get; set; }

    public DateTime PriceUpdatedAt { get; set; } = DateTime.UtcNow;
}
