namespace FuelStations.Dtos;

/// <summary>
/// Вид пального з довідника.
/// Fuel type from the reference table.
/// </summary>
public record FuelTypeDto(
    string Code,
    string Name,
    bool IsActive,
    DateTime CreatedAt);

/// <summary>
/// ID АЗС (вікно каси) з описом.
/// Station ID (cashier window) with a description.
/// </summary>
public record TerminalDto(
    int Number,
    string? Description);

/// <summary>
/// АЗС у списку: усі її ID і основні дані, без пального.
/// Station in a list: all its IDs and main info, without fuels.
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
/// Fuel at a station: type code and name, price, availability, price change date.
/// </summary>
public record StationFuelDto(
    string FuelTypeCode,
    string FuelTypeName,
    decimal Price,
    bool IsAvailable,
    DateTime PriceUpdatedAt);

/// <summary>
/// АЗС повністю: дані, усі ID, пальне й ціни.
/// Full station: info, all IDs, fuels and prices.
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
/// Page of results: items, page number and size, total count.
/// </summary>
public record PagedResult<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
