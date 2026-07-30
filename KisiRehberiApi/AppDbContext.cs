using Microsoft.EntityFrameworkCore;
using KisiRehberiApi;
using System.Diagnostics.Contracts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    //üstte yazılan koddaki veritabanı sqllite olsun dosya adı contacts.db olsun kısmındaki ayar paketini alıp veritabanı motoruna teslim eder 

    public DbSet<Contact> Contacts { get; set; }
    public DbSet<User> Users {get; set;}
    //DbSet ef core'a özgü veri tipi.Veritabanındaki table kavramının karşılığı
    //<contact> tablonun içindeki tutulacak verinin türünü söyler.contacts da veritabanındaki tablonun resmi gerçek adı
    // get set klasik bu tablonun okunup yazılabilir
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Occupation> Occupations {get;set;}//Databasedeki Occupations tablosunun entity framework karşılığı
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    base.OnModelCreating(modelBuilder);
    modelBuilder.Entity<AuditLog>()
        .HasIndex(a => a.CreatedAt);

    modelBuilder.Entity<Contact>()
      .HasOne(c=>c.Occupation) // 1 kişinin 1 mesleği
      .WithMany(o=>o.Contacts) // 1 mesleğin çok kişisi 
      .HasForeignKey(c=>c.OccupationId)//ForeingKey kolonu
      .OnDelete(DeleteBehavior.SetNull);//meslek silinirse kişide OccupationId = null
    }  

}