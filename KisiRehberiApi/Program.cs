using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

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
         options.UseSqlite("Data Source=contacts.db"));

var app = builder.Build();
app.UseCors("AllowAngular");

//veritabanındaki tüm kişileri çeker ve angulara liste olarak gönderir 
app.MapGet("/api/contacts", async (AppDbContext db) =>
{
   return Results.Ok(await db.Contacts.ToListAsync());
});
//adresten gelen id değerini alıp findasync(id) metodu ile veritabanında arar ve onu getirir.Eğer kişi yoksa 404 hata mesajı döner
app.MapGet("/api/contacts/{id:int}", async (int id,AppDbContext db) =>
{
    var contact = await db.Contacts.FindAsync(id);
    return contact is not null? Results.Ok(contact): Results.NotFound("Kişi bulunamadı.");
});

//anguların gönderdiği form verisini(contact) alır.EF core belleğindeki Contacts listesine ekler savechangesasync ile contacts.db dosyasına fiziksel olarak işler
//ve 201 created mesajı ile yeni oluşturulan kişiyi döner.
app.MapPost("/api/contacts",async (Contact contact,AppDbContext db) =>
{
    db.Contacts.Add(contact);
    await db.SaveChangesAsync();
    return Results.Created($"/api/contacts/{contact.Id}",contact);
});

//id si verilen kişiyi bulur,yeni bilgileri günceller ve veritabanına kaydeder.
app.MapPut("/api/contacts/{id:int}", async (int id, Contact updatedContact, AppDbContext db) =>
{
   var contact = await db.Contacts.FindAsync(id);
   if(contact is null) return Results.NotFound("Kişi Bulunamadı.");
   contact.FirstName = updatedContact.FirstName;
   contact.LastName = updatedContact.LastName;
   contact.PhoneNumber = updatedContact.PhoneNumber;
   contact.Email = updatedContact.Email;
   await db.SaveChangesAsync();
   return Results.Ok(contact); 
   //adresteki id ile veritabanındaki mevcut kişi bulunur gelen verileri updatedContact üzerine yazar ve saveChangesasync ile contacts.db dosyasını günceller.
});

//idsi verilen kişiyi veritabanından bulup kaldırır
app.MapDelete("/api/contacts/{id:int}",async (int id, AppDbContext db)=>
{
    var contact = await db.Contacts.FindAsync(id);
    if (contact is null) return Results.NotFound("Kişi bulunamadı.");

    db.Contacts.Remove(contact);
    await db.SaveChangesAsync();
    return Results.Ok("Kişi başarıyla silindi");
    //adresten gelen id ile kişiyi veritabanında arar,bulunca remove ile silme listesine alır ve savechangesasync() ile kaydı contacts.db dosyasından kalıcı olarak siler. 

});
app.Run();