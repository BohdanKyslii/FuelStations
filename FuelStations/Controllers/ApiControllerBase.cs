using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FuelStations.Controllers;

/// <summary>
/// Спільні помічники для контролерів API: єдиний формат помилок і безпечне збереження.
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
    /// Зберігає зміни; повертає false, якщо порушено обмеження БД
    /// (наприклад, дублікат, створений паралельним запитом).
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
    /// Нормалізує бізнес-код: прибирає пробіли й переводить у верхній регістр
    /// ("a95 " → "A95").
    /// </summary>
    protected static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

}
