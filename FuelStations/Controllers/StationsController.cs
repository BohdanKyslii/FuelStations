using FuelStations.Data;
using FuelStations.Dtos;
using FuelStations.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FuelStations.Controllers;

/// <summary>
/// Stations API. Every station is addressed by any of its 4-digit station IDs.
/// </summary>
[ApiController]
[Route("api/stations")]
public class StationsController(AppDbContext db) : ApiControllerBase
{
    /// <summary>
    /// Returns a paginated list of stations filtered by city, status,
    /// available fuel type code and station ID.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<StationListItemDto>>> GetStations(
        [FromQuery] StationQuery query)
    {
        IQueryable<Station> stations = db.Stations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            var city = query.City.Trim();
            stations = stations.Where(s => EF.Functions.ILike(s.City, city));
        }

        if (query.IsActive is { } isActive)
        {
            stations = stations.Where(s => s.IsActive == isActive);
        }

        if (!string.IsNullOrWhiteSpace(query.FuelType))
        {
            var fuelTypeCode = NormalizeCode(query.FuelType);
            stations = stations.Where(s =>
                s.Fuels.Any(f => f.FuelTypeCode == fuelTypeCode && f.IsAvailable));
        }

        if (query.Number is { } stationNumber)
        {
            stations = stations.Where(s => s.Terminals.Any(t => t.Number == stationNumber));
        }

        var totalCount = await stations.CountAsync();

        var items = await stations
            .OrderBy(s => s.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(s => new StationListItemDto(
                s.Terminals.OrderBy(t => t.Number).Select(t => t.Number).ToList(),
                s.Name,
                s.City,
                s.Address,
                s.IsActive,
                s.CreatedAt))
            .ToListAsync();

        return Ok(new PagedResult<StationListItemDto>(
            items, query.Page, query.PageSize, totalCount));
    }

    /// <summary>
    /// Returns a station by any of its station IDs, with all its IDs, fuels and prices.
    /// </summary>
    [HttpGet("{number:int}")]
    public async Task<ActionResult<StationDetailsDto>> GetStation(int number)
    {
        var station = await db.Stations
            .AsNoTracking()
            .Where(s => s.Terminals.Any(t => t.Number == number))
            .AsSplitQuery()
            .Select(s => new StationDetailsDto(
                s.Name,
                s.City,
                s.Address,
                s.Description,
                s.IsActive,
                s.CreatedAt,
                s.Terminals
                    .OrderBy(t => t.Number)
                    .Select(t => new TerminalDto(t.Number, t.Description))
                    .ToList(),
                s.Fuels
                    .OrderBy(f => f.FuelTypeCode)
                    .Select(f => new StationFuelDto(
                        f.FuelTypeCode,
                        f.FuelType.Name,
                        f.Price,
                        f.IsAvailable,
                        f.PriceUpdatedAt))
                    .ToList()))
            .FirstOrDefaultAsync();

        return station is null ? StationNotFound(number) : Ok(station);
    }

    /// <summary>
    /// Creates a station with one or more station IDs.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<StationDetailsDto>> CreateStation(CreateStationRequest request)
    {
        // Values are guaranteed by [Required] and [Range] validation.
        var numbers = request.Terminals.Select(t => t.Number!.Value).ToList();

        if (numbers.Count != numbers.Distinct().Count())
        {
            return BadRequestProblem("ID АЗС у запиті не повинні повторюватися.");
        }

        var takenNumbers = await db.StationTerminals
            .Where(t => numbers.Contains(t.Number))
            .Select(t => t.Number)
            .ToListAsync();

        if (takenNumbers.Count > 0)
        {
            return BadRequestProblem(
                $"ID АЗС вже закріплені за іншою АЗС: {string.Join(", ", takenNumbers)}.");
        }

        var name = request.Name.Trim();

        if (await db.Stations.AnyAsync(s => s.Name == name))
        {
            return BadRequestProblem($"АЗС з назвою «{name}» вже існує.");
        }

        var station = new Station
        {
            Name = name,
            City = request.City.Trim(),
            Address = request.Address.Trim(),
            Description = request.Description?.Trim(),
            IsActive = request.IsActive ?? true,
            Terminals = request.Terminals
                .Select(t => new StationTerminal
                {
                    Number = t.Number!.Value,
                    Description = t.Description?.Trim(),
                })
                .ToList(),
        };

        db.Stations.Add(station);

        if (!await TrySaveChangesAsync(db))
        {
            return BadRequestProblem("ID АЗС або назва вже використовуються.");
        }

        return CreatedAtAction(
            nameof(GetStation), new { number = numbers.Min() }, ToDetailsDto(station));
    }

    /// <summary>
    /// Updates station info. Station IDs are immutable and cannot be changed here.
    /// </summary>
    [HttpPut("{number:int}")]
    public async Task<ActionResult<StationDetailsDto>> UpdateStation(
        int number,
        UpdateStationRequest request)
    {
        var station = await db.Stations
            .Include(s => s.Terminals)
            .Include(s => s.Fuels)
                .ThenInclude(f => f.FuelType)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Terminals.Any(t => t.Number == number));

        if (station is null)
        {
            return StationNotFound(number);
        }

        var name = request.Name.Trim();

        if (await db.Stations.AnyAsync(s => s.Name == name && s.Id != station.Id))
        {
            return BadRequestProblem($"АЗС з назвою «{name}» вже існує.");
        }

        station.Name = name;
        station.City = request.City.Trim();
        station.Address = request.Address.Trim();
        station.Description = request.Description?.Trim();
        station.IsActive = request.IsActive ?? station.IsActive;

        if (!await TrySaveChangesAsync(db))
        {
            return BadRequestProblem($"АЗС з назвою «{name}» вже існує.");
        }

        return Ok(ToDetailsDto(station));
    }

    /// <summary>
    /// Deletes the whole station (all its station IDs and fuels) by any of its station IDs.
    /// </summary>
    [HttpDelete("{number:int}")]
    public async Task<IActionResult> DeleteStation(int number)
    {
        var station = await db.Stations
            .FirstOrDefaultAsync(s => s.Terminals.Any(t => t.Number == number));

        if (station is null)
        {
            return StationNotFound(number);
        }

        db.Stations.Remove(station);
        await db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Adds a new station ID to an existing station (e.g. a new cashier window).
    /// </summary>
    [HttpPost("{number:int}/terminals")]
    public async Task<ActionResult<TerminalDto>> AddTerminal(int number, TerminalRequest request)
    {
        var stationId = await FindStationIdAsync(number);

        if (stationId is null)
        {
            return StationNotFound(number);
        }

        var newNumber = request.Number!.Value;

        if (await db.StationTerminals.AnyAsync(t => t.Number == newNumber))
        {
            return BadRequestProblem($"ID АЗС {newNumber} вже закріплений за АЗС.");
        }

        var terminal = new StationTerminal
        {
            Number = newNumber,
            StationId = stationId.Value,
            Description = request.Description?.Trim(),
        };

        db.StationTerminals.Add(terminal);

        if (!await TrySaveChangesAsync(db))
        {
            return BadRequestProblem($"ID АЗС {newNumber} вже закріплений за АЗС.");
        }

        return CreatedAtAction(
            nameof(GetStation),
            new { number = newNumber },
            new TerminalDto(terminal.Number, terminal.Description));
    }

    /// <summary>
    /// Adds an active fuel type with price to a station.
    /// </summary>
    [HttpPost("{number:int}/fuels")]
    public async Task<ActionResult<StationFuelDto>> AddFuel(int number, AddFuelRequest request)
    {
        var stationId = await FindStationIdAsync(number);

        if (stationId is null)
        {
            return StationNotFound(number);
        }

        var fuelTypeCode = NormalizeCode(request.FuelTypeCode);
        var fuelType = await db.FuelTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Code == fuelTypeCode);

        if (fuelType is null)
        {
            return BadRequestProblem(
                $"Невідомий вид пального {fuelTypeCode}. Створіть його через POST /api/fuel-types.");
        }

        if (!fuelType.IsActive)
        {
            return BadRequestProblem($"Вид пального {fuelTypeCode} деактивовано.");
        }

        var alreadyExists = await db.StationFuels
            .AnyAsync(f => f.StationId == stationId && f.FuelTypeCode == fuelTypeCode);

        if (alreadyExists)
        {
            return BadRequestProblem($"Вид пального {fuelTypeCode} вже додано на АЗС з ID {number}.");
        }

        var fuel = new StationFuel
        {
            StationId = stationId.Value,
            FuelTypeCode = fuelTypeCode,
            Price = request.Price!.Value,
            IsAvailable = request.IsAvailable,
        };

        db.StationFuels.Add(fuel);

        if (!await TrySaveChangesAsync(db))
        {
            return BadRequestProblem($"Вид пального {fuelTypeCode} вже додано на АЗС з ID {number}.");
        }

        var dto = new StationFuelDto(
            fuel.FuelTypeCode, fuelType.Name, fuel.Price, fuel.IsAvailable, fuel.PriceUpdatedAt);

        return CreatedAtAction(nameof(GetStation), new { number }, dto);
    }

    /// <summary>
    /// Updates price and/or availability of a fuel at a station.
    /// </summary>
    [HttpPatch("{number:int}/fuels/{fuelTypeCode}")]
    public async Task<ActionResult<StationFuelDto>> UpdateFuel(
        int number,
        string fuelTypeCode,
        UpdateFuelRequest request)
    {
        if (request.Price is null && request.IsAvailable is null)
        {
            return BadRequestProblem(
                "Передайте нову ціну (price) або ознаку наявності (isAvailable).");
        }

        var stationId = await FindStationIdAsync(number);

        if (stationId is null)
        {
            return StationNotFound(number);
        }

        var code = NormalizeCode(fuelTypeCode);
        var fuel = await db.StationFuels
            .Include(f => f.FuelType)
            .FirstOrDefaultAsync(f => f.StationId == stationId && f.FuelTypeCode == code);

        if (fuel is null)
        {
            return NotFoundProblem($"Вид пального {code} на АЗС з ID {number} не знайдено.");
        }

        if (request.Price is { } newPrice && newPrice != fuel.Price)
        {
            fuel.Price = newPrice;
            fuel.PriceUpdatedAt = DateTime.UtcNow;
        }

        if (request.IsAvailable is { } isAvailable)
        {
            fuel.IsAvailable = isAvailable;
        }

        await db.SaveChangesAsync();

        return Ok(ToFuelDto(fuel));
    }

    /// <summary>
    /// Resolves a station ID to the internal station key, or null if the ID is unknown.
    /// </summary>
    private Task<int?> FindStationIdAsync(int number) =>
        db.StationTerminals
            .Where(t => t.Number == number)
            .Select(t => (int?)t.StationId)
            .FirstOrDefaultAsync();

    /// <summary>
    /// Maps a station fuel to DTO. Requires the FuelType navigation to be loaded.
    /// </summary>
    private static StationFuelDto ToFuelDto(StationFuel fuel) =>
        new(fuel.FuelTypeCode, fuel.FuelType.Name, fuel.Price, fuel.IsAvailable, fuel.PriceUpdatedAt);

    private static StationDetailsDto ToDetailsDto(Station station) =>
        new(
            station.Name,
            station.City,
            station.Address,
            station.Description,
            station.IsActive,
            station.CreatedAt,
            station.Terminals
                .OrderBy(t => t.Number)
                .Select(t => new TerminalDto(t.Number, t.Description))
                .ToList(),
            station.Fuels
                .OrderBy(f => f.FuelTypeCode)
                .Select(ToFuelDto)
                .ToList());

    private ObjectResult StationNotFound(int number) =>
        NotFoundProblem($"АЗС з ID {number} не знайдено.");
}
