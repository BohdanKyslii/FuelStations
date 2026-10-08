namespace FuelStations.Dtos;

/// <summary>
/// Вид пального з довідника.
/// </summary>
public record FuelTypeDto(
    string Code,
    string Name,
    bool IsActive,
    DateTime CreatedAt);

/// <summary>
/// ID АЗС (вікно каси) з описом.
/// </summary>
public record TerminalDto(
    int Number,
    string? Description);

/// <summary>
/// АЗС у списку: усі її ID і основні дані, без пального.
/// </summary>
public record StationListItemDto(
    List<int> Numbers,
    string Name,
    string City,
    string Address,
    bool IsActive,
    DateTime CreatedAt);

/// <summary>
/// Пальне на АЗС: код і назва виду, ціна, наявність, дата зміни ціни.
/// </summary>
public record StationFuelDto(
    string FuelTypeCode,
    string FuelTypeName,
    decimal Price,
    bool IsAvailable,
    DateTime PriceUpdatedAt);

/// <summary>
/// АЗС повністю: дані, усі ID, пальне й ціни.
/// </summary>
public record StationDetailsDto(
    string Name,
    string City,
    string Address,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    List<TerminalDto> Terminals,
    List<StationFuelDto> Fuels);

/// <summary>
/// Сторінка результатів: елементи, номер і розмір сторінки, загальна кількість.
/// </summary>
public record PagedResult<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
