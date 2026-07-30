# Kişi Rehberi Uygulaması

Şirket içi kişileri ekleyip listeleyebileceğiniz, güncelleyip silebileceğiniz bir rehber uygulamasıdır. JWT ile admin girişi, meslek seçimi ve işlem logları (audit) paneli bulunur.

**Stack:** Angular · ASP.NET Core Minimal API · Entity Framework Core · PostgreSQL

---

## Klasör yapısı

```
staj1/
├── kisi-rehberi-ui/    # Frontend (Angular)
└── KisiRehberiApi/     # Backend (Minimal API + EF Core)
```

---

## Gereksinimler

- [.NET SDK](https://dotnet.microsoft.com/download) (8 veya üzeri önerilir)
- [Node.js](https://nodejs.org/) ve npm
- [PostgreSQL](https://www.postgresql.org/)
- (İsteğe bağlı) Visual Studio / VS Code / Cursor

---

## Kurulum — Backend

1. `KisiRehberiApi` klasörüne girin.
2. `appsettings.json` içindeki `ConnectionStrings:DefaultConnection` değerini kendi PostgreSQL bilgilerinize göre düzenleyin  
   (`Host`, `Port`, `Database`, `Username`, `Password`).
3. Migration’ları uygulayın (proje klasöründeyken):

   ```bash
   dotnet ef database update
   ```

4. API’yi başlatın:

   ```bash
   dotnet run
   ```

5. API adresi varsayılan olarak: `http://localhost:5163`

> Not: İlk çalıştırmada meslek listesi boşsa uygulama seed ile örnek meslekleri ekler.

---

## Kurulum — Frontend

1. `kisi-rehberi-ui` klasörüne girin.
2. Bağımlılıkları yükleyin:

   ```bash
   npm install
   ```

3. API adresi kodda `http://localhost:5163` olarak geçiyor. Backend farklı portta çalışıyorsa ilgili servis dosyalarındaki URL’leri güncelleyin.
4. Uygulamayı başlatın:

   ```bash
   ng serve
   ```

5. Tarayıcıda açın: `http://localhost:4200`

---

## İlk kullanım


### 1) Kayıt ol / Giriş yap

![Giriş veya kayıt ekranı](docs/screenshots/01-login.png)

Uygulama açılınca login ekranı gelir. Hesabınız yoksa **Kayıt Ol** ile admin oluşturabilir, ardından kullanıcı adı ve şifre ile **Giriş Yap** dersiniz. Başarılı girişte JWT token kaydedilir ve rehber listesine yönlendirilirsiniz.

### 2) Kişi listesi

![Kişi listesi ekranı](docs/screenshots/02-contact-list.png)

Rehberde kayıtlı kişiler kartlar halinde listelenir (ad, soyad, telefon, e-posta, meslek). Üstten arama yapabilir; **Yeni Kişi Ekle**, **Log Paneli** veya **Çıkış Yap** butonlarını kullanabilirsiniz.

### 3) Yeni kişi ekleme

![Kişi ekleme formu](docs/screenshots/03-add-contact.png)

**Yeni Kişi Ekle** ile forma gidin. Ad, soyad, telefon ve e-posta zorunludur; meslek dropdown’dan seçilebilir. Kayıt başarılı olunca listeye dönersiniz.

![Zorunlu alan uyarıları](docs/screenshots/03b-add-contact-validation.png)

Formdaki zorunlu alanlar boş bırakılamaz. Boş bırakılıp kaydedilmeye çalışılırsa (veya alana dokunulup boş geçilirse) sistem uyarı gösterir ve kişi eklemeye izin vermez; kaydet butonu da form geçersizken kilitli kalır.

### 4) Kişi düzenleme

![Kişi düzenleme formu](docs/screenshots/04-edit-contact.png)

Listede **Düzenle** ile mevcut kaydın formu açılır. Bilgileri güncelleyip kaydedin.

![E-posta unique uyarısı](docs/screenshots/04b-edit-email-unique.png)

Bir kişinin e-posta adresi başka bir kişide kullanılamaz (e-posta unique olmalıdır). Güncellemede başka bir kayıttaki e-posta yazılırsa sistem uyarı verir ve işlem tamamlanmaz.

### 5) Kişi silme

![Silme onayı](docs/screenshots/05-delete-confirm.png)

**Sil** butonuna basınca onay sorulur. Onaylarsanız kişi rehberden kalıcı olarak silinir.

### 6) Log paneli

![Log paneli](docs/screenshots/06-logs.png)

**Log Paneli**nde admin işlemleri izlenir: soldan admin seçilir, sağda pasta grafik; altta giriş/çıkış ve kişi işlem tabloları görünür.

---


## Ana özellikler

- Kişi CRUD (ekle, listele, güncelle, sil)
- Form doğrulama ve kullanıcı bildirimleri
- JWT ile admin girişi / çıkışı
- Meslek seçimi (ayrı `Occupations` tablosu)
- İşlem logları ve log paneli (grafik + tablolar)

---

## Önemli API uçları

| Metot | Adres | Açıklama |
|--------|--------|----------|
| POST | `/api/register` | Admin kayıt |
| POST | `/api/login` | Giriş + JWT |
| POST | `/api/logout` | Çıkış kaydı |
| GET/POST/PUT/DELETE | `/api/contacts` | Kişi CRUD |
| GET | `/api/occupations` | Meslek listesi |
| GET | `/api/logs` | Sayfalı log listesi |
| GET | `/api/logs/stats` | Admin özeti |
| GET | `/api/logs/stats/{userName}` | Admin işlem dağılımı |

Tüm korumalı uçlar için istekte `Authorization: Bearer <token>` gerekir.

---


## Karşılaşılan sorunlar ve çözümler

### Kaydet butonuna ard arda basınca birden fazla kayıt oluşması
Kişi ekleme/güncelleme formunda kullanıcı **Kaydet** butonuna hızlıca birden fazla kez basınca aynı istek tekrarlanıyor ve birden fazla kayıt oluşuyordu.

**Çözüm:** Form tarafında `isSubmitting` kilidi eklendi. İstek giderken buton disabled oluyor ve ikinci tıklama işleme alınmıyor; böylece çift kayıt engelleniyor.

### Log paneline girince uygulamanın donması / sunucunun yanıt vermemesi
Log sayısı arttıkça panel açılışında tüm `AuditLogs` kayıtları tek seferde çekiliyordu. Büyük JSON ve sıralama yüzünden API ve arayüz kilitlenir gibi görünüyordu.

**Çözüm:** `GET /api/logs` endpoint’ine sayfalama eklendi (`page` / `pageSize`, `Skip` / `Take`). Okuma sorgularında `AsNoTracking` kullanıldı. Böylece her istekte sınırlı sayıda kayıt geliyor.

---

## Gelecekte eklenecekler

- **Meslek ekleme / yönetim ekranı:** Backend’de meslek endpoint’leri (`GET/POST/DELETE /api/occupations`) mevcut; frontend’de henüz meslek ekleme/silme sekmesi yok. İleride admin panelinden meslek yönetimi eklenebilir.
- **Excel / CSV içe ve dışa aktarma:** Rehberi dosyadan yükleme ve listeyi Excel/CSV olarak indirme.
- **Kişi kartı detay popup’ı:** Listedeki kişi kartına tıklanınca özetin ötesinde detaylı bilgilerin bir popup / modal ile gösterilmesi.
- **RBAC**  Role Based Access Control sistemini temel olarak karşılasakta daha farklı roller ve yetkilendirmeler yapılabilir.