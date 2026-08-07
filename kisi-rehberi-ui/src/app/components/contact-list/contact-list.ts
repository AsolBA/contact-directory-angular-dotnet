import { FormsModule } from '@angular/forms';
import { Component, OnInit } from '@angular/core';
import * as GC from '@mescius/spread-sheets';
import '@mescius/spread-sheets-io';
import { saveAs } from 'file-saver';
import { CommonModule } from '@angular/common';
import { PaginatorModule } from 'primeng/paginator';
import { Router, RouterModule } from '@angular/router'; // Router buraya eklendi
import { ContactService, Contact } from '../../services/contact';
import { HttpClient } from '@angular/common/http';


// amacımız ihtiyacımız olan angular modüllerini, apı servisimizi ve rehber listesini tutacak boş diziyi tanımlamak
@Component({
  selector: 'app-contact-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PaginatorModule], // HTML'de döngüler ve linkler için gerekli
  templateUrl: './contact-list.html',
  styleUrl: './contact-list.css',
})
export class ContactList implements OnInit {
  contacts: Contact[] = []; // backendden gelecek kişiler bu dizinde saklanacak
  searchText: string = '';  // arama kutusundaki yazı
  first: number = 0;
  rows: number = 9;

  constructor(
    private contactService: ContactService, // projede api aracısını kullanmak için yazılmış
    private router: Router, // Çıkış yapınca Login ekranına yönlendirme yapabilmek için eklendi
    private http:HttpClient
  ) {}

  ngOnInit(): void { // ngoninit sayfa hazır olunca bir kez çalışan yer
    this.loadContacts(); // sayfa açıldığı an listeyi dolduruyor
  }

  // tüm kişileri backendden çeken metod
  loadContacts(): void {
    this.contactService.getContacts().subscribe({ // subscribe cevap hemen gelmez ondan var gelince haber ver demek
      next: (data) => {
        this.contacts = data; // gelen listenin diziye atandığı yer 
      },
      error: (err) => {
        console.error('Kişiler yüklenirken hata oluştu:', err);
      }
    });
  }
  
  get filteredContacts(): Contact[] { // eğer kutu boşsa herkesi ver arama kutusu
    const q = this.searchText.trim().toLowerCase();
    if (!q) {
      return this.contacts;
    }
    return this.contacts.filter(c => // Bu şartlardan birini sağlaması yeterli veya var
      c.firstName.toLowerCase().includes(q) ||
      c.lastName.toLowerCase().includes(q) ||
      c.phoneNumber.includes(q) ||
      c.email.toLowerCase().includes(q) ||
      (c.occupation?.name?? '').toLowerCase().includes(q) ||
      (c.city ?? '').toLowerCase().includes(q)
    );
  }
 
  get paginatedContacts(): Contact[] {
  return this.filteredContacts.slice(this.first, this.first + this.rows);
}

onPageChange(event: any): void {
  this.first = event.first;
  this.rows = event.rows;
}
  
  onSearchChange(): void {
  this.first = 0;
}

  deleteContact(id: number): void {
    // Kullanıcıya tarayıcı üzerinden onay sorusu çıkarıyoruz
    if (confirm('Bu kişiyi rehberden silmek istediğinize emin misiniz?')) {
      this.contactService.deleteContact(id).subscribe({
        next: () => {
          // silme başarılı ise ekranı yenilemek için liste apiye tekrar yükleniyor
          this.loadContacts();
        },
        error: (err) => {
          console.error('Silme işlemi sırasında hata oluştu:', err);
        }
      });
    }
  }

  // ÇIKIŞ YAP (LOGOUT)
  onLogout(): void {
  if (confirm('Çıkış yapmak istediğinize emin misiniz?')) {
    this.http.post('http://localhost:5163/api/logout', {}).subscribe({
      next: () => {
        localStorage.removeItem('token');
        this.router.navigate(['/login']);
      },
      error: () => {
        localStorage.removeItem('token');
        this.router.navigate(['/login']);
      }
    });
  }
}

  //LOG PANELİNE YÖNLENDİRME
  goToLogs(): void {
    this.router.navigate(['/logs']);
  }

  // // Üstteki Excel Export butonuna basınca çalışır; ekranda tablo göstermeden .xlsx indirir
  exportToExcel(): void {
    //arama boşsa tüm kişiler,doluysa sadece eşleşenler
    const list = this.filteredContacts;
    //indirecek kimse yoksa kullanıcıya haber verip metottan çıkar return ile 
    if (list.length === 0) {
      alert('İndirilecek kişi bulunamadı.');
      return;
    }

  //Workbook bir HTML elemanına bağlanmak ister bizde bunu ekranda göstermemek için gizli div oluşturuyoruz
    const host = document.createElement('div');
    host.style.position = 'fixed';
    host.style.left = '-9999px'; //ekranın dışına koyuyoruz 
    host.style.width = '1px';
    host.style.height = '1px';
    document.body.appendChild(host);//kullanıcı görmüyor ama kütüphane yine de hostum var diyip çalışıyor

    //burda kişiler adında boş bir excel açıyoruz
    const spread = new GC.Spread.Sheets.Workbook(host, { sheetCount: 1 });
    const sheet = spread.getActiveSheet();
    sheet.name('Kisiler');


    // exceldeki başlıklarımız yani sütunlarımızı yazıyoruz
    const headers = ['Ad', 'Soyad', 'Telefon', 'E-posta', 'Meslek','Şehir'];
    sheet.setArray(0, 0, [headers]); //sol üstten başla Excel karşılığı a1 oluyor ilk satıra sütun isimlerini yaz

    // Veri satırları// birinci satırdan başlayarak yani en üstten ikinci gelen bilgileri yazıyor 
    const rows = list.map(c => [
      c.firstName,
      c.lastName,
      c.phoneNumber,
      c.email,
      c.occupation?.name ?? '',//meslek gelmezse boş bırakıyor
      c.city ?? ''
    ]);
    sheet.setArray(1, 0, rows);

    // Başlık kalın
    const headerRange = sheet.getRange(0, 0, 1, headers.length);
    headerRange.font('bold 12px Arial');

    for (let col = 0; col < headers.length; col++) {
      sheet.setColumnWidth(col, 140); //kolonlar dar olmasın diye 
    }

    spread.export(
      (blob: any) => { //export başarılı olunca kütüphane bunu çağırır
        saveAs(blob, 'Kisiler_Export.xlsx'); //bu blob'u kullanıcıya Kisiler_Export adıyla indir,tarayıcı indirmeyi başlatır
        spread.destroy();//workbook'u kapat bellekten sil.
        host.remove();//gizli div'i sayfadan çıkarır 
      },
      (e: any) => {//bir şey ters giderse kütüphane bunu çağırır 
        console.error('Excel export hatası:', e);//geliştirici konsoluna hata yazılır 
        spread.destroy(); // hata olsa da geçici motoru ve ve gizli div'i temizler remove ve destroy kısmı
        host.remove();
      },
      { fileType: GC.Spread.Sheets.FileType.excel } //çıktı formatı ne olacak bunun belirlenmesi için kullandığımız komut 
    );
  }

}