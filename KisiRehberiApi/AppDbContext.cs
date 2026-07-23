using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    //üstte yazılan koddaki veritabanı sqllite olsun dosya adı contacts.db olsun kısmındaki ayar paketini alıp veritabanı motoruna teslim eder 

    public DbSet<Contact> Contacts { get; set; }
    //DbSet ef core'a özgü veri tipi.Veritabanındaki table kavramının karşılığı
    //<contact> tablonun içindeki tutulacak verinin türünü söyler.contacts da veritabanındaki tablonun resmi gerçek adı
    // get set klasik bu tablonun okunup yazılabilir
}