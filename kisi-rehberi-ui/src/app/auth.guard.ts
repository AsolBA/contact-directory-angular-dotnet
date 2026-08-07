import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);//kodun içinde sayfa yönlendirmesi yapabilmek için router servisi inject ediliyor
  const token = localStorage.getItem('token');//localde kayıtlı olan token bilgisini alıyoruz 

  // eğer tarayıcıda token varsa(giriş yapılmışsa)
  if (token) {
    return true;//kullanıcının geçmek istediği sayfaya izin verilir
  }

  // Token yoksa kullanıcıyı login sayfasına gitsin ve geçişi engelle
  router.navigate(['/login']);
  return false;
};