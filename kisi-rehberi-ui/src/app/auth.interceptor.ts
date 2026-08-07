import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // localStorage'dan kaydettiğimiz token'ı alıyoruz
  const token = localStorage.getItem('token');//kullanıcı giriş yapınca login component içindeki jwt token hafızadan okunur.

  // Eğer hafızada token varsa (yani giriş yapılmışa) bu bloğa girer.
  if (token) {
    const authReq = req.clone({//angulardan gelen http isteği direkt req değiştirilemez.bu yüzden isteğin kopyasını oluşturuyoruz
      setHeaders: {
        Authorization: `Bearer ${token}`//isteğin header kısmına 'authorization:bearer<token> bilgisini koyar 
      }
    });
    return next(authReq);//tokenli yeni istek backende gidiyor
  }

  return next(req);//login olunmamışsa isteği direkt geldiği şekil yollar
};