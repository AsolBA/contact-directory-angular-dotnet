import { Component, OnInit } from '@angular/core'; 
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ContactService, Contact, Occupation } from '../../services/contact';

@Component({
  selector: 'app-contact-form', //bileşenin htmlde hangi etiketle çağırılacağını belirler
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule],//kullanılacak kütüphaneleri bağlar
  templateUrl: './contact-form.html', //kod hangi html sayfası ile çalışacak
  styleUrls: ['./contact-form.css']//kod hangi css sayfası ile çalışacak
})
export class ContactFormComponent implements OnInit { //sınıf değişkenleri
  contactForm!: FormGroup; 
  isEditMode: boolean = false; //sayfanın hangi modda olduğunu söyler
  contactId?: number; //düzenlenecek kişinin idsi
  errorMessage: string = '';
  successMessage: string = '';
  isSubmitting  = false;
  occupations: Occupation[] = [];//dropdowndaki meslek listesi

  constructor(
    private fb: FormBuilder, //angular bunları otomatik verir.
    private contactService: ContactService, 
    private router: Router, 
    private route: ActivatedRoute 
  ) { } 

  // formu her şeyden önce ramde oluşturuyoruz, angular arayüzü çizmeye çalışırken undefined hatası vermesin
  ngOnInit(): void {   //en başta kurulması sebebi htlm çizilirken form henüz yoksa patlar,önce form hazır olsun diye
    this.contactForm = this.fb.group({
      firstName: ['', [Validators.required]],        
      lastName: ['', [Validators.required]], //alan adı , başlangıç değeri , kurallar
      phoneNumber: ['', [Validators.required,Validators.pattern(/^[0-9]{10,11}$/)]],
      email: ['', [Validators.required, Validators.email]], // eposta dogrulaması
      city :[''],
      occupationId:[null] //meslek seçimi
    });
    
  this.contactService.getOccupations().subscribe({
       next:(data) => this.occupations = data,  
       error:() => this.errorMessage = 'Meslek listesi yüklenemedi.'
  });
  
    // url her değiştiğinde arka planda değişikliği canlı olarak dinler
    this.route.params.subscribe(params => {
      if (params['id']) { // eğer urlde bir id varsa düzenleme moduna geç
        this.isEditMode = true; // sayfa edit moduna geçiyor
        this.contactId = +params['id'];  // string gelen id'yi sayıya dönüştürüyor
        this.loadContactData(this.contactId); // verileri çekebilmek için load contact data fonksiyonunu tetikliyor 
      }
    });
  }

  // api'den o id'li kişiyi iste
  loadContactData(id: number): void {
    this.contactService.getContactById(id).subscribe({ // api den ilgili id deki kişiyi getiriyor 
      next: (contact) => this.contactForm.patchValue(contact),  // gelen kişinin bilgilerini form alanlarına otomatik dolduruyor
      error: () => this.errorMessage = 'Kişi bilgileri yüklenemedi.' // api bağlantısı kurulamaz veya ilgili kişiye ulaşılamazsa hata mesajını ekrana veriyor
    });
  }

  onPhoneKeyPress(event:KeyboardEvent):void {
    const char = event.key;
    //rakam değilse yazmayı engelle
    if(!/^[0-9]$/.test(char)){
      event.preventDefault();
    }
  }

  onSubmit(): void {
    // formdaki geçersiz alan kontrolü yapılıyor
    if (this.contactForm.invalid) {
      this.contactForm.markAllAsTouched(); // formdaki alanlara dokunulmuş süsü veriliyor böylece htlmdeki bu alan zorunludur uyarıları ekranda beliriyor
      return;
    }
    
    if(this.isSubmitting)
    {
      return;   //çift tıklama kilidi 
    }
    this.isSubmitting = true;

    // form geçerliyse içindeki veriler paket haline getiriliyor
    const contactData: Contact = this.contactForm.value;

    // edit modu açık ve elimizde contactId varsa kullanıcının mevcut kişiyi güncellemek istediğini anlamak ve apıye put isteğinde bulunmak amaçlanıyor
    if (this.isEditMode && this.contactId) {
      // güncelleme metodu olan put çağırılıyor api servisinden
      this.contactService.updateContact(this.contactId, contactData).subscribe({
        next: () => {
          // işlem başarılıysa mesajı göster 2 saniye sonra kullanıcıyı listeye yönlendir tekrar
          this.successMessage = 'Kişi başarıyla güncellendi. Listeye yönlendiriliyorsunuz...';
          setTimeout(() => this.router.navigate(['/contacts']), 2000); 
        },
        error: (err) => {
        if (err.status === 409) {
        this.errorMessage = err.error || 'Bu e-posta adresi başka bir kişide kayıtlı.';
        } else {
        this.errorMessage = 'Güncelleme işlemi sırasında bir hata oluştu.';
  }
  this.isSubmitting = false;
}
      });
    }
    else {
      // kullanıcı düzenleme modunda değilse kullanıcının yeni kişi eklemek istediğini algılayıp post isteğinde bulunmak
      this.contactService.addContact(contactData).subscribe({
        next: () => {
          // İşlem başarılıysa bildirim ver ve 2 saniye sonra listeye yönlendir
          this.successMessage = 'Kişi başarıyla rehbere kaydedildi. Listeye yönlendiriliyorsunuz...';
          setTimeout(() => this.router.navigate(['/contacts']), 2000);
        },
        
        error: (err) => {
        if (err.status === 409) {
        this.errorMessage = err.error || 'Bu e-posta adresi zaten kayıtlı.';
        } else {
        this.errorMessage = 'Kayıt işlemi sırasında bir hata oluştu.';
        }
        this.isSubmitting = false;
}
      });
    }
  }
} 

 
