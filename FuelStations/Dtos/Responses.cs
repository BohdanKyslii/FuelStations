namespace FuelStations.Dtos;

public record FuelTypeDto(
    string Code,
    string Name,
    bool IsActive,
    DateTime CreatedAt);

public record TerminalDto(
    int Number,
    string? Description);

public record StationListItemDto(
    List<int> Numbers,
    string Name,
    string City,
    string Address,
    bool IsActive,
    DateTime CreatedAt);

public record StationFuelDto(
    string FuelTypeCode,
    string FuelTypeName,
    decimal Price,
    bool IsAvailable,
    DateTime PriceUpdatedAt);

public record StationDetailsDto(
    string Name,
    string City,
    string Address,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    List<TerminalDto> Terminals,
    List<StationFuelDto> Fuels);

public record PagedResult<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
