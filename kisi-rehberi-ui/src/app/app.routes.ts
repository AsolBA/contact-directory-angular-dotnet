import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { ContactList } from './components/contact-list/contact-list'; 
import { ContactFormComponent } from './components/contact-form/contact-form';
import { Logs } from './logs/logs'; 
import { authGuard } from './auth.guard'; 

export const routes: Routes = [
  // Uygulama ilk açıldığında doğrudan logine yönlendiriyor
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  
  // Login Rotası koruma yok
  { path: 'login', component: LoginComponent },

  // Sadece token varsa girilebilen rotalar
  { path: 'contacts', component: ContactList, canActivate: [authGuard] },
  { path: 'add-contact', component: ContactFormComponent, canActivate: [authGuard] },
  { path: 'edit-contact/:id', component: ContactFormComponent, canActivate: [authGuard] }, 

  // 📊 LOG PANELI ROTASI
  { path: 'logs', component: Logs, canActivate: [authGuard] }
];