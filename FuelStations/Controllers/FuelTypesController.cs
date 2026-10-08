using FuelStations.Data;
using FuelStations.Dtos;
using FuelStations.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FuelStations.Controllers;

/// <summary>
/// Довідник видів пального. Кожен вид адресується своїм кодом (наприклад, A95).
/// Fuel types reference API. Each fuel type is addressed by its code (e.g. A95).
/// </summary>
[ApiController]
[Route("api/fuel-types")]
public class FuelTypesController(AppDbContext db) : ApiControllerBase
{
    /// <summary>
    /// Повертає види пального. Неактивні включаються лише з includeInactive=true.
    /// Returns fuel types. Inactive ones are included only when includeInactive=true.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<FuelTypeDto>>> GetFuelTypes(
        [FromQuery] bool includeInactive = false)
    {
        IQueryable<FuelType> fuelTypes = db.FuelTypes.AsNoTracking();

        if (!includeInactive)
        {
            fuelTypes = fuelTypes.Where(f => f.IsActive);
        }

        var items = await fuelTypes
            .OrderBy(f => f.Code)
            .Select(f => new FuelTypeDto(f.Code, f.Name, f.IsActive, f.CreatedAt))
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Повертає вид пального за кодом (регістр не важливий).
    /// Returns a fuel type by its code (case-insensitive).
    /// </summary>
    [HttpGet("{code}")]
    public async Task<ActionResult<FuelTypeDto>> GetFuelType(string code)
    {
        var normalizedCode = NormalizeCode(code);

        var fuelType = await db.FuelTypes
            .AsNoTracking()
            .Where(f => f.Code == normalizedCode)
            .Select(f => new FuelTypeDto(f.Code, f.Name, f.IsActive, f.CreatedAt))
            .FirstOrDefaultAsync();

        return fuelType is null ? FuelTypeNotFound(normalizedCode) : Ok(fuelType);
    }

    /// <summary>
    /// Створює новий вид пального. Код приводиться до верхнього регістру.
    /// Creates a new fuel type. The code is normalized to upper case.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FuelTypeDto>> CreateFuelType(CreateFuelTypeRequest request)
    {
        var code = NormalizeCode(request.Code);
        var name = request.Name.Trim();

        if (await db.FuelTypes.AnyAsync(f => f.Code == code))
        {
            return BadRequestProblem($"Вид пального з кодом {code} вже існує.");
        }

        if (await db.FuelTypes.AnyAsync(f => f.Name == name))
        {
            return BadRequestProblem($"Вид пального з назвою «{name}» вже існує.");
        }

        var fuelType = new FuelType { Code = code, Name = name };
        db.FuelTypes.Add(fuelType);

        if (!await TrySaveChangesAsync(db))
        {
            return BadRequestProblem($"Вид пального з кодом {code} або назвою «{name}» вже існує.");
        }

        return CreatedAtAction(nameof(GetFuelType), new { code }, ToDto(fuelType));
    }

    /// <summary>
    /// Змінює назву та активність. Код змінити не можна.
    /// Updates the name and active flag. The code is immutable.
    /// </summary>
    [HttpPut("{code}")]
    public async Task<ActionResult<FuelTypeDto>> UpdateFuelType(
        string code,
        UpdateFuelTypeRequest request)
    {
        var normalizedCode = NormalizeCode(code);
        var fuelType = await db.FuelTypes.FirstOrDefaultAsync(f => f.Code == normalizedCode);

        if (fuelType is null)
        {
            return FuelTypeNotFound(normalizedCode);
        }

        var name = request.Name.Trim();

        if (await db.FuelTypes.AnyAsync(f => f.Name == name && f.Code != normalizedCode))
        {
            return BadRequestProblem($"Вид пального з назвою «{name}» вже існує.");
        }

        fuelType.Name = name;
        fuelType.IsActive = request.IsActive ?? fuelType.IsActive;

        if (!await TrySaveChangesAsync(db))
        {
            return BadRequestProblem($"Вид пального з назвою «{name}» вже існує.");
        }

        return Ok(ToDto(fuelType));
    }

    /// <summary>
    /// Видаляє вид пального, який не продається на жодній АЗС.
    /// Вид, що продається, можна лише деактивувати.
    /// Deletes a fuel type that isn't used at any station.
    /// Used fuel types must be deactivated instead.
    /// </summary>
    [HttpDelete("{code}")]
    public async Task<IActionResult> DeleteFuelType(string code)
    {
        var normalizedCode = NormalizeCode(code);
        var fuelType = await db.FuelTypes.FirstOrDefaultAsync(f => f.Code == normalizedCode);

        if (fuelType is null)
        {
            return FuelTypeNotFound(normalizedCode);
        }

        if (await db.StationFuels.AnyAsync(f => f.FuelTypeCode == normalizedCode))
        {
            return BadRequestProblem(
                $"Вид пального {normalizedCode} продається на АЗС, тому його не можна видалити. "
                + "Деактивуйте його (isActive: false).");
        }

        db.FuelTypes.Remove(fuelType);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static FuelTypeDto ToDto(FuelType fuelType) =>
        new(fuelType.Code, fuelType.Name, fuelType.IsActive, fuelType.CreatedAt);

    private ObjectResult FuelTypeNotFound(string code) =>
        NotFoundProblem($"Вид пального з кодом {code} не знайдено.");
}
