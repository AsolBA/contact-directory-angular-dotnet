import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Occupation{
  id : number;
  name : string;
}

//verilerin hangi alanda oluşacağını belirler bir kişi nasıl gözükecek sorusu
//veri kaydedilmiyor sadece verinin sadece şeklini belirtir interface
export interface Contact {
  id?: number; // ekleme yaparken artacak diye ? bıraktık
  firstName: string;
  lastName: string;
  phoneNumber: string;
  email: string;
  city?:string | null;
  occupationId?: number | null; // seçilen mesleğin id'si
  occupation?: Occupation | null; // backend Include ile gelebilir
}

@Injectable({
  providedIn: 'root' // Bu servisin tüm projede tek bir merkezden yönetilmesini sağlıyor
})                   //liste ve form ayrı ayrı servis açmıyor ortak bunu kullanıyor
export class ContactService {
  // .NET Minimal API port adresimiz
  private apiUrl = 'http://localhost:5163/api/contacts'; //bu adres sadece bu dosyada kullanılsın dışardan değiştirilmesin diye private
  private occupationsUrl = 'http://localhost:5163/api/occupations'; // meslek endpoint'i
  constructor(private http: HttpClient) { }//httpclient kütüphanesini bu servis içinde kullanabilmek için yapıcı metoda bağlar

  //Tüm kişileri backend'den çeker 
  getContacts(): Observable<Contact[]> {
    return this.http.get<Contact[]>(this.apiUrl);
  }
  // observable dönüş tipi:sunucuya soru sorulunca anında cevap gelmiyor cevap gelince haber ediyor.
  // urlnin sonuna id ekleyerek sadece o kişiye ait detayları backendden çeker
  getContactById(id: number): Observable<Contact> {
    return this.http.get<Contact>(`${this.apiUrl}/${id}`);
  }

  //yeni kişi ekler 
  addContact(contact: Contact): Observable<Contact> {
    return this.http.post<Contact>(this.apiUrl, contact);
  }

  // belirtilen id li kişinin üzerine,formdaki yeni bilgileri yazması için put isteği atar
  updateContact(id: number, contact: Contact): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, contact);
  }

  getOccupations(): Observable<Occupation[]>{
    return this.http.get<Occupation[]>(this.occupationsUrl);
  }

  // Kişiyi listeden silmek için delete isteği atar 
  deleteContact(id: number): Observable<string> {
    return this.http.delete<string>(`${this.apiUrl}/${id}`, { responseType: 'text' as 'json' });
    //backendden text cevabı geleceği için hata vermemesi için eklendi
  }
}