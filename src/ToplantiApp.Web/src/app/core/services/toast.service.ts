import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

/**
 * Uygulama genelinde kullanilacak toast ara servisi.
 * Tum basari/hata bildirimleri bu servis uzerinden gosterilir.
 */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly defaultTitle = {
    success: 'Başarılı',
    error: 'Hata',
    info: 'Bilgi',
    warning: 'Uyarı'
  };

  constructor(private toastr: ToastrService) {}

  success(message: string, title = this.defaultTitle.success): void {
    this.toastr.success(message, title);
  }

  error(message: string, title = this.defaultTitle.error): void {
    this.toastr.error(message, title);
  }

  info(message: string, title = this.defaultTitle.info): void {
    this.toastr.info(message, title);
  }

  warning(message: string, title = this.defaultTitle.warning): void {
    this.toastr.warning(message, title);
  }

  clear(): void {
    this.toastr.clear();
  }
}
