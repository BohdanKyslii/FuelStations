using System.ComponentModel.DataAnnotations;
using FuelStations.Common;

namespace FuelStations.Dtos;

/// <summary>
/// Query parameters for station search with pagination.
/// </summary>
public class StationQuery
{
    public string? City { get; set; }

    public bool? IsActive { get; set; }

    /// <summary>
    /// Fuel type code available at the station (e.g. A95).
    /// </summary>
    public string? FuelType { get; set; }

    /// <summary>
    /// Find a station by any of its station IDs.
    /// </summary>
    public int? Number { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Номер сторінки має бути не меншим за 1.")]
    public int Page { get; set; } = 1;

    [Range(1, Pagination.MaxPageSize, ErrorMessage = "Розмір сторінки має бути від 1 до 100.")]
    public int PageSize { get; set; } = Pagination.DefaultPageSize;
}

/// <summary>
/// Payload for creating a fuel type.
/// </summary>
public class CreateFuelTypeRequest
{
    [Required(ErrorMessage = "Вкажіть код виду пального.")]
    [RegularExpression(
        "^[A-Za-z0-9_]{2,20}$",
        ErrorMessage = "Код має містити 2–20 латинських літер, цифр або «_», наприклад A95.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть назву виду пального.")]
    [MaxLength(50, ErrorMessage = "Назва не може бути довшою за 50 символів.")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Editable fuel type info. The code is immutable and not part of this payload.
/// </summary>
public class UpdateFuelTypeRequest
{
    [Required(ErrorMessage = "Вкажіть назву виду пального.")]
    [MaxLength(50, ErrorMessage = "Назва не може бути довшою за 50 символів.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть, чи активний вид пального (isActive).")]
    public bool? IsActive { get; set; }
}

/// <summary>
/// Station ID with an optional description.
/// </summary>
public class TerminalRequest
{
    [Required(ErrorMessage = "Вкажіть ID АЗС.")]
    [Range(1000, 9999, ErrorMessage = "ID АЗС має бути чотиризначним числом (1000–9999).")]
    public int? Number { get; set; }

    [MaxLength(100, ErrorMessage = "Опис не може бути довшим за 100 символів.")]
    public string? Description { get; set; }
}

/// <summary>
/// Editable station info. Station IDs are immutable and not part of this payload.
/// </summary>
public class UpdateStationRequest
{
    [Required(ErrorMessage = "Вкажіть назву АЗС.")]
    [MaxLength(100, ErrorMessage = "Назва не може бути довшою за 100 символів.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть місто.")]
    [MaxLength(100, ErrorMessage = "Назва міста не може бути довшою за 100 символів.")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть адресу АЗС.")]
    [MaxLength(250, ErrorMessage = "Адреса не може бути довшою за 250 символів.")]
    public string Address { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Опис не може бути довшим за 500 символів.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Вкажіть статус роботи АЗС (isActive).")]
    public bool? IsActive { get; set; }
}

/// <summary>
/// Payload for creating a station: station info plus at least one station ID.
/// </summary>
public class CreateStationRequest : UpdateStationRequest
{
    [Required(ErrorMessage = "Вкажіть ID АЗС.")]
    [MinLength(1, ErrorMessage = "Вкажіть щонайменше один ID АЗС.")]
    public List<TerminalRequest> Terminals { get; set; } = [];
}

/// <summary>
/// Payload for adding a fuel type to a station.
/// </summary>
public class AddFuelRequest
{
    [Required(ErrorMessage = "Вкажіть код виду пального.")]
    [MaxLength(20, ErrorMessage = "Код виду пального не може бути довшим за 20 символів.")]
    public string FuelTypeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть ціну.")]
    [Range(0.01, 100_000, ErrorMessage = "Ціна має бути більшою за нуль.")]
    public decimal? Price { get; set; }

    public bool IsAvailable { get; set; } = true;
}

/// <summary>
/// Partial update of fuel price and/or availability.
/// </summary>
public class UpdateFuelRequest
{
    [Range(0.01, 100_000, ErrorMessage = "Ціна має бути більшою за нуль.")]
    public decimal? Price { get; set; }

    public bool? IsAvailable { get; set; }
}
