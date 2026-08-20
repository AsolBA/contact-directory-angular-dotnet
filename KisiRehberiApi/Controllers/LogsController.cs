using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KisiRehberiApi.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize]
public class LogsController : ControllerBase
{
    private readonly AppDbContext _db;
    
    public LogsController(AppDbContext db)
    {
        _db =db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 50)
    {
        var totalCount = await _db.AuditLogs.CountAsync();
        var logs = await _db.AuditLogs
            .AsNoTracking()
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page-1)*pageSize)
            .Take(pageSize)
            .ToListAsync();
        return Ok(new{totalCount,logs});
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _db.AuditLogs
            .GroupBy(l => l.UserName)
            .Select(g =>new{UserName = g.Key, Count = g.Count() })
            .ToListAsync();
        return Ok(stats);
    }

    [HttpGet("stats/{userName}")]
    public async Task<IActionResult> GetStatsByUser(string userName)
    {
        var stats = await _db.AuditLogs
            .AsNoTracking()
            .Where(l => l.UserName == userName)
            .GroupBy(l => l.ActionType)
            .Select(g => new { ActionType = g.Key, Count = g.Count() })
            .ToListAsync();
        return Ok(stats);
    }
}