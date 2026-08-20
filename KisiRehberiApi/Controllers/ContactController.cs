using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using KisiRehberiApi.Services;

namespace KisiRehberiApi.Controllers;

[ApiController]
[Route("api/contacts")]
[Authorize]
public class ContactsController: ControllerBase
{
    private const string AdminName = "Admin";
    private readonly AppDbContext _db;
    private readonly ILogService _logService;

    public ContactsController(AppDbContext db,ILogService logService)
    {
        _db=db;
        _logService = logService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var contacts = await _db.Contacts
            .Include(c => c.Occupation)
            .AsNoTracking()
            .ToListAsync();
        return Ok(contacts);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var contact = await _db.Contacts
            .Include(c => c.Occupation)
            .FirstOrDefaultAsync(c => c.Id == id);
        return contact is not null ? Ok(contact):NotFound("Kişi bulunamadı.");
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(Contact contact)
    {
        var emailExists = await _db.Contacts
            .AnyAsync(c=>c.Email.ToLower() == contact.Email.ToLower());
        if (emailExists)
        {
            return Conflict("Bu e-posta adresi zaten kayıtlı");
        }
        _db.Contacts.Add(contact);
        await _db.SaveChangesAsync();

        string username = User.FindFirstValue(ClaimTypes.Name) ?? AdminName;
        await _logService.LogAsync(username, "CREATE_CONTACT", $"'{contact.FirstName} {contact.LastName}' rehbere eklendi.");
        return Created($"/api/contacts/{contact.Id}",contact); 
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id,Contact updatedContact)
    {
        var contact = await _db.Contacts.FindAsync(id);
        if (contact is null) return NotFound("Kişi Bulunamadı.");

        var emailExists = await _db.Contacts
            .AnyAsync(c => c.Email.ToLower() == updatedContact.Email.ToLower()
                        && c.Id != id);

        if (emailExists)
        {
            return Conflict("Bu e-posta adresi başka bir kişide kayıtlı.");
        }

        contact.FirstName = updatedContact.FirstName;
        contact.LastName = updatedContact.LastName;
        contact.PhoneNumber = updatedContact.PhoneNumber;
        contact.Email = updatedContact.Email;
        contact.OccupationId = updatedContact.OccupationId;
        contact.City = updatedContact.City;
        await _db.SaveChangesAsync();

        string username = User.FindFirstValue(ClaimTypes.Name) ?? AdminName;
        await _logService.LogAsync(username, "UPDATE_CONTACT", $"ID: {id} olan '{contact.FirstName} {contact.LastName}' bilgileri güncellendi.");
        return Ok(contact);    
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var contact = await _db.Contacts.FindAsync(id);
        if(contact is null) return NotFound("Kişi bulunamadı.");

        _db.Contacts.Remove(contact);
        await _db.SaveChangesAsync();

        string username = User.FindFirstValue(ClaimTypes.Name) ?? AdminName;
        await _logService.LogAsync(username, "DELETE_CONTACT", $"ID: {id} olan '{contact.FirstName} {contact.LastName}' kişisi silindi.");
        return Ok("Kişi başarıyla silindi");
    }




}