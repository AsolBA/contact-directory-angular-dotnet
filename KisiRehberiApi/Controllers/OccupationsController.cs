using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KisiRehberiApi.Controllers;

[ApiController]
[Route("api/occupations")]
[Authorize]
public class OccupationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public OccupationsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Occupations
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Occupation occupation)
    {
        var exists = await _db.Occupations
            .AnyAsync(o => o.Name.ToLower() == occupation.Name.ToLower());

        if (exists)
            return Conflict("Bu meslek zaten kayıtlı.");

        _db.Occupations.Add(occupation);
        await _db.SaveChangesAsync();
        return Created($"/api/occupations/{occupation.Id}", occupation);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var occupation = await _db.Occupations.FindAsync(id);
        if (occupation is null)
            return NotFound("Meslek bulunamadı.");

        _db.Occupations.Remove(occupation);
        await _db.SaveChangesAsync();
        return Ok("Meslek silindi.");
    }
}
