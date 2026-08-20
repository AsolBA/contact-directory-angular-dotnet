using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using KisiRehberiApi;
using KisiRehberiApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
        policy.WithOrigins("http://localhost:4200")
           .AllowAnyMethod()
           .AllowAnyHeader();
    });
});
    
//sen sqllite veri tabanı kullanacaksın ve dosyanın adı da contacts.db olacak dedik dosyaya
builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ILogService,LogService>();
builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    if (!await db.Occupations.AnyAsync())//başta tablo boş ise 
    {
        db.Occupations.AddRange( //sabit meslekler  
            new Occupation {Name = "Yazılım Geliştirici"},
            new Occupation {Name = "Analist"},
            new Occupation {Name = "İnsan Kaynakları"},
            new Occupation {Name = "Muhasebe"},
            new Occupation {Name = "Satış"},
            new Occupation {Name = "Diğer"}
        );
    await db.SaveChangesAsync();//bellekteki eklemeyi Databaseye yazar
    }
}

app.UseCors("AllowAngular");
app.UseAuthentication(); // 1. Önce kimsine bak (Kimlik doğrula)
app.UseAuthorization();  // 2. Sonra yetkisine bak (İzin ver/verme)
app.MapControllers();



await app.RunAsync();