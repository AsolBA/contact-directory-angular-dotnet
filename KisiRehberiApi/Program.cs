using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using BCrypt.Net;
using KisiRehberiApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

// --- JWT KIMLIK DOGRULAMA SERVISI ---
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);//metin byte'a çeviliyor ve imza anahtarı key'e atanıyor

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => // kimlik doğrulama jwt ile yapılacak diyoruz gelen istekte bearer token varsa token kontrol edilir
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,//tokeni kim çıkardı
        ValidateAudience = true,//kime verildi
        ValidateLifetime = true,//süresi doldu mu
        ValidateIssuerSigningKey = true,//imza doğru mu
        ValidIssuer = jwtSettings["Issuer"],//doğru kabul edilecek yayıncı
        ValidAudience = jwtSettings["Audience"],//doğru kabul edilecek hedef
        IssuerSigningKey = new SymmetricSecurityKey(key)//doğru kabul edilecek imza anahtarı
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    //angular apiye rahatça erişebilsin diye cors izni veriyoruz
    options.AddPolicy("AllowAngular",policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
    
//sen sqllite veri tabanı kullanacaksın ve dosyanın adı da contacts.db olacak dedik dosyaya
builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ILogService,LogService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Occupations.Any())//başta tablo boş ise 
    {
        db.Occupations.AddRange( //sabit meslekler  
            new Occupation {Name = "Yazılım Geliştirici"},
            new Occupation {Name = "Analist"},
            new Occupation {Name = "İnsan Kaynakları"},
            new Occupation {Name = "Muhasebe"},
            new Occupation {Name = "Satış"},
            new Occupation {Name = "Diğer"}
        );
    db.SaveChanges();//bellekteki eklemeyi Databaseye yazar
    }
}

app.UseCors("AllowAngular");
app.UseAuthentication(); // 1. Önce kimsine bak (Kimlik doğrula)
app.UseAuthorization();  // 2. Sonra yetkisine bak (İzin ver/verme)

//veritabanındaki tüm kişileri çeker ve angulara liste olarak gönderir 
app.MapGet("/api/contacts", async (AppDbContext db) =>
{
   var contacts = await db.Contacts
    .Include(c => c.Occupation) // meslek bilgisini de yükle
    .AsNoTracking()
    .ToListAsync();
return Results.Ok(contacts);
}).RequireAuthorization();

//adresten gelen id değerini alıp findasync(id) metodu ile veritabanında arar ve onu getirir.Eğer kişi yoksa 404 hata mesajı döner
app.MapGet("/api/contacts/{id:int}", async (int id,AppDbContext db) =>
{
    var contact = await db.Contacts
    .Include(c => c.Occupation)
    .FirstOrDefaultAsync(c => c.Id == id);
    return contact is not null? Results.Ok(contact): Results.NotFound("Kişi bulunamadı.");
}).RequireAuthorization();

//anguların gönderdiği form verisini(contact) alır.EF core belleğindeki Contacts listesine ekler savechangesasync ile contacts.db dosyasına fiziksel olarak işler
//ve 201 created mesajı ile yeni oluşturulan kişiyi döner.
app.MapPost("/api/contacts",async (Contact contact,AppDbContext db,ClaimsPrincipal user,ILogService logService) =>
{
    var emailExists = await db.Contacts
        .AnyAsync(c => c.Email.ToLower() == contact.Email.ToLower());
        //bu şartı sağlayan en az 1 kişi var mı
    if (emailExists)
    {
        return Results.Conflict("Bu e-posta adresi zaten kayıtlı.");
    }

    db.Contacts.Add(contact);
    await db.SaveChangesAsync();
    // 🪵 KİŞİ EKLEME LOG KAYDI
    //tokenden admin kaydı alınıyor
    string username = user.FindFirstValue(ClaimTypes.Name) ?? "Admin";
    await logService.LogAsync(username, "CREATE_CONTACT", $"'{contact.FirstName} {contact.LastName}' rehbere eklendi.");
    return Results.Created($"/api/contacts/{contact.Id}",contact);
}).RequireAuthorization();

//id si verilen kişiyi bulur,yeni bilgileri günceller ve veritabanına kaydeder.
app.MapPut("/api/contacts/{id:int}", async (int id, Contact updatedContact, AppDbContext db,ClaimsPrincipal user,ILogService logService) =>
{
   var contact = await db.Contacts.FindAsync(id);
   if(contact is null) return Results.NotFound("Kişi Bulunamadı.");

   var emailExists = await db.Contacts
    .AnyAsync(c => c.Email.ToLower() == updatedContact.Email.ToLower()
                && c.Id != id);//kendisi hariç var mı kontrolü

   if (emailExists)
   {
    return Results.Conflict("Bu e-posta adresi başka bir kişide kayıtlı.");
   }
   contact.FirstName = updatedContact.FirstName;
   contact.LastName = updatedContact.LastName;
   contact.PhoneNumber = updatedContact.PhoneNumber;
   contact.Email = updatedContact.Email;
   contact.OccupationId = updatedContact.OccupationId;
   await db.SaveChangesAsync();
   string username = user.FindFirstValue(ClaimTypes.Name) ?? "Admin";
   await logService.LogAsync(username, "UPDATE_CONTACT", $"ID: {id} olan '{contact.FirstName} {contact.LastName}' bilgileri güncellendi.");
   return Results.Ok(contact); 
   //adresteki id ile veritabanındaki mevcut kişi bulunur gelen verileri updatedContact üzerine yazar ve saveChangesasync ile contacts.db dosyasını günceller.
}).RequireAuthorization();

//idsi verilen kişiyi veritabanından bulup kaldırır
app.MapDelete("/api/contacts/{id:int}",async (int id, AppDbContext db,ClaimsPrincipal user,ILogService logService)=>
{
    var contact = await db.Contacts.FindAsync(id);
    if (contact is null) return Results.NotFound("Kişi bulunamadı.");

    db.Contacts.Remove(contact);
    await db.SaveChangesAsync();
    string username = user.FindFirstValue(ClaimTypes.Name) ?? "Admin";
    await logService.LogAsync(username, "DELETE_CONTACT", $"ID: {id} olan '{contact.FirstName} {contact.LastName}' kişisi silindi.");
    return Results.Ok("Kişi başarıyla silindi");
    //adresten gelen id ile kişiyi veritabanında arar,bulunca remove ile silme listesine alır ve savechangesasync() ile kaydı contacts.db dosyasından kalıcı olarak siler. 
}).RequireAuthorization();

//tüm meslekleri listeleme dropdown için
app.MapGet("/api/occupations", async (AppDbContext db)=>
{
    var list = await db.Occupations
        .AsNoTracking()
        .OrderBy(o=>o.Name)
        .ToListAsync();
    return Results.Ok(list);
}).RequireAuthorization();

//yeni meslek ekle
app.MapPost("/api/occupations", async (Occupation occupation, AppDbContext db)=>
{
    //aynı isim var mı kontrolü
    var exists = await db.Occupations
        .AnyAsync(o=>o.Name.ToLower()== occupation.Name.ToLower());
    
    if(exists)
       return Results.Conflict("Bu meslek zaten kayıtlı.");

    db.Occupations.Add(occupation);
    await db.SaveChangesAsync();
    return Results.Created($"/api/occupations/{occupation.Id}",occupation);
}).RequireAuthorization();

//meslek sil
app.MapDelete("/api/occupations/{id:int}",async (int id,AppDbContext db)=>
{
    var occupation = await db.Occupations.FindAsync(id);
    if(occupation is null)
       return Results.NotFound("Meslek bulunamadı.");

    db.Occupations.Remove(occupation);
    await db.SaveChangesAsync();
    return Results.Ok("Meslek silindi.");
}).RequireAuthorization();

// ADMIN KAYIT ENDPOINT'I
app.MapPost("/api/register", async (UserRegisterDto dto, AppDbContext db) =>
{
    if (await db.Users.AnyAsync(u => u.Username == dto.Username))//bu kullanıcı adı daha önce alındı mı
    {
        return Results.BadRequest("Bu kullanıcı adı zaten mevcut.");
    }

    string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password); //metin hashlenir db çalınsa bile şifreyi kolay göremez

    var user = new User
    {
        Username = dto.Username,
        PasswordHash = passwordHash //yeni admin nesnesi ile hashli şifre
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();
    //token burda oluşmaz sadece kayıt yapılır
    return Results.Ok("Admin kullanıcısı başarıyla oluşturuldu."); //tabloya ekle kaydet
});

// ADMIN GİRİŞ (LOGIN) ENDPOINT'I
app.MapPost("/api/login", async (UserLoginDto dto, AppDbContext db, IConfiguration config, ILogService logService) =>
{
    // Kullanıcıyı veritabanında ara
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
    if (user == null)
    {
        return Results.BadRequest("Kullanıcı bulunamadı veya şifre yanlış.");
    }

    // Girilen şifre ile veritabanındaki hash'i karşılaştır
    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
    if (!isPasswordValid)
    {
        return Results.BadRequest("Kullanıcı bulunamadı veya şifre yanlış.");//bu isimde bir kullanıcı var mı
    }

    // token üretim alanı jwt
    var jwtConf = config.GetSection("Jwt");
    var keyBytes = Encoding.UTF8.GetBytes(jwtConf["Key"]!);

    var claims = new[]
    {   //tokenin içine yazılan küçük bilgiler id username rol gibi 
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, "Admin")
    };

    var tokenDescriptor = new SecurityTokenDescriptor
    {   //tokenin tarifi gibi bir şey.içinde claims bulunuyor,kaç gün geçerli,kim üretti,kime gidiyor,
        //hangi anahtar ve algoritma ile imzalandı
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddDays(1), // Token 1 gün geçerli
        Issuer = jwtConf["Issuer"],
        Audience = jwtConf["Audience"],
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
    };


    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor); //tariften gerçek token üretiliyor ve stringe çevriliyor 
    var tokenString = tokenHandler.WriteToken(token);
    // 🪵 LOGIN LOG KAYDI
    await logService.LogAsync(user.Username, "LOGIN", "Sisteme başarılı giriş yapıldı.");
    // Frontend'e JWT Token'ı döndürüyoruz
    return Results.Ok(new { token = tokenString });
});
//log getiren endpoint
app.MapGet("/api/logs", async (AppDbContext db, int page = 1, int pageSize = 50) =>
{
    var totalCount = await db.AuditLogs.CountAsync();//tabloda toplam kaç log var
    var logs = await db.AuditLogs 
        .AsNoTracking()
        .OrderByDescending(l => l.CreatedAt) //en yeni log en üstte
        .Skip((page - 1) * pageSize) //önceki sayfayı atla örn page 2 50 kayıt atla
        .Take(pageSize) //bu sayfadan en fazla 50 kayıt al
        .ToListAsync();
    return Results.Ok(new { totalCount, logs }); // hem toplam sayı hem o sayfanın listesini döndür
}).RequireAuthorization();

//grafik chart için istatistik dönen endpoint
app.MapGet("/api/logs/stats", async (AppDbContext db) =>
{
    var stats = await db.AuditLogs
        .GroupBy(l => l.UserName) //logları admin adına göre grupla
        .Select(g => new { UserName = g.Key, Count = g.Count() }) //o adminin toplam log sayısı
        .ToListAsync();
    return Results.Ok(stats);
} //örnek çıktı  { "userName": "admin", "count": 2 }
).RequireAuthorization();//yetkisiz işlem yapılmasın diye 

// Seçilen adminin işlem tiplerine göre sayılarını döner (pasta grafik için)
app.MapGet("/api/logs/stats/{userName}", async (string userName, AppDbContext db) =>
{
    // Sadece bu adminin loglarını al, ActionType'a göre grupla, her tipin adedini say
    var stats = await db.AuditLogs
        .AsNoTracking()
        .Where(l => l.UserName == userName)
        .GroupBy(l => l.ActionType)
        .Select(g => new { ActionType = g.Key, Count = g.Count() })
        .ToListAsync();

    return Results.Ok(stats);
}).RequireAuthorization();
//çıkış anını kaydeder
app.MapPost("/api/logout", async (ClaimsPrincipal user, ILogService logService) =>
{
    var username = user.Identity?.Name ?? "Bilinmeyen";
    await logService.LogAsync(username, "LOGOUT", "Sistemden çıkış yapıldı.");
    return Results.Ok("Çıkış kaydedildi.");
}).RequireAuthorization();

app.Run();