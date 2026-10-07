using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FuelStations.Controllers;

/// <summary>
/// Shared helpers for API controllers: unified error responses and safe saving.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    protected ObjectResult NotFoundProblem(string detail) =>
        Problem(detail: detail, statusCode: StatusCodes.Status404NotFound,
            title: "Не знайдено");

    protected ObjectResult BadRequestProblem(string detail) =>
        Problem(detail: detail, statusCode: StatusCodes.Status400BadRequest,
            title: "Некоректні дані");

    /// <summary>
    /// Saves changes; returns false if a database constraint was violated
    /// (e.g. a duplicate created by a concurrent request).
    /// </summary>
    protected static async Task<bool> TrySaveChangesAsync(DbContext db)
    {
        try
        {
            await db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    /// <summary>
    /// Normalizes a business code: trims spaces and converts to upper case ("a95 " → "A95").
    /// </summary>
    protected static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

}
