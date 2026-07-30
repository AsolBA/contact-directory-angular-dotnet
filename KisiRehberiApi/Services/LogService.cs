using System;
using System.Threading.Tasks;


public class LogService : ILogService
{
    private readonly AppDbContext _context;
    public LogService(AppDbContext context)
    {
        _context = context;  //appdbcontext alınır logu tabloya bununla yazarız
    }
    // asıl log yazan sınıf
    //yeni bir auditlog oluşturur,kullanıcı adı boşsa systemguest yazar veritabanına kaydeder.
    public async Task LogAsync(string userName, string actionType,string details)
    {
        var log = new AuditLog  //yeni audit log oluşturur
        {//isim boşsa guest yazar tip + detay + zamanı doldurur auditlogs tablosuna kaydeder
          UserName = string.IsNullOrEmpty(userName) ? "System / Guest" : userName, 
          ActionType = actionType,
          Details = details,
          CreatedAt = DateTime.UtcNow 
        };
        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    } //program.cs log yaz der burası da dbye satır atar.
}