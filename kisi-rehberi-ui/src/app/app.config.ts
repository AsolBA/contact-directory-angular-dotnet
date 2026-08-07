import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './auth.interceptor';
import { provideAnimations } from '@angular/platform-browser/animations'; // 
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';

export const appConfig: ApplicationConfig = {
  providers: [
  provideZoneChangeDetection({ eventCoalescing: true }), 
  provideRouter(routes),
  provideHttpClient(withInterceptors([authInterceptor])),
  provideAnimations(),
  providePrimeNG({
    theme: {
      preset: Aura
    },
    license:'eyJpZCI6ImNlNDQxNDM1LWJlYTktNGMzMi1hNDVlLWEwN2VlNmI3NWFiOSIsInByb2R1Y3QiOiJwcmltZXVpIiwidGllciI6ImNvbW11bml0eSIsInR5cGUiOiJkZXYiLCJpYXQiOjE3ODQ4OTEyNDksImV4cCI6MTgxNjQyNzI0OX0.pou6agkrav2ZNinr3gvxom24Rm48Xo1wVxXQ1VTH8tWxOtTL7PQmKVRXYxqQ1GAbQe4J0XwZEGpG9eph7F0oDw'
  })
]
};