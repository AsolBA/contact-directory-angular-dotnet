import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Router } from '@angular/router';

// PrimeNG Bileşenleri
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    HttpClientModule,
    ButtonModule,
    DividerModule,
    InputTextModule
  ]
})
export class LoginComponent {
  username: string = '';
  password: string = '';
  errorMessage: string = '';
  successMessage: string = '';

  private apiUrl = 'http://localhost:5163/api';

  constructor(private http: HttpClient, private router: Router) {}

  onLogin() {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.username || !this.password) {
      this.errorMessage = 'Lütfen kullanıcı adı ve şifre giriniz.';
      return;
    }

    const body = { username: this.username, password: this.password };

    this.http.post<{ token: string }>(`${this.apiUrl}/login`, body).subscribe({
      next: (response) => {
        localStorage.setItem('token', response.token);
        this.successMessage = 'Giriş başarılı! Yönlendiriliyorsunuz...';
        
        setTimeout(() => {
          this.router.navigate(['/contacts']);
        }, 1000);
      },
      error: (err) => {
        this.errorMessage = err.error || 'Giriş başarısız! Kullanıcı adı veya şifre hatalı.';
      }
    });
  }

  onSignUp() {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.username || !this.password) {
      this.errorMessage = 'Lütfen kayıt için kullanıcı adı ve şifre giriniz.';
      return;
    }

    const body = { username: this.username, password: this.password };//backende gidecek veri json formatına dönüşüyor

    this.http.post(`${this.apiUrl}/register`, body, { responseType: 'text' }).subscribe({
      next: () => {
        this.successMessage = 'Admin kaydı başarılı! Şimdi giriş yapabilirsiniz.';
      },
      error: (err) => {
        this.errorMessage = err.error || 'Kayıt olunamadı! Kullanıcı adı zaten mevcut olabilir.';
      }
    });
  }
}