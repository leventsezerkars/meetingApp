import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { getApiErrorMessage } from '../../core/utils/api-error.utils';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="row justify-content-center">
      <div class="col-md-5">
        <div class="card shadow">
          <div class="card-body p-4">
            <h3 class="text-center mb-4">Giriş Yap</h3>
            <form (ngSubmit)="onSubmit()">
              <div class="mb-3">
                <label class="form-label">E-posta</label>
                <input type="email" class="form-control" [(ngModel)]="email" name="email" required>
              </div>
              <div class="mb-3">
                <label class="form-label">Şifre</label>
                <input type="password" class="form-control" [(ngModel)]="password" name="password" required>
              </div>
              <button type="submit" class="btn btn-primary w-100" [disabled]="loading">
                {{ loading ? 'Giriş yapılıyor...' : 'Giriş Yap' }}
              </button>
            </form>
            <p class="text-center mt-3">
              Hesabınız yok mu? <a routerLink="/register">Kayıt Ol</a>
            </p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent implements OnInit {
  email = '';
  password = '';
  loading = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private toast: ToastService,
    private title: Title
  ) {}

  ngOnInit(): void {
    this.title.setTitle('Giriş Yap | Toplantı Yönetimi');
  }

  onSubmit(): void {
    this.loading = true;
    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: () => {
        this.toast.success('Giriş başarılı.');
        this.router.navigate(['/meetings']);
      },
      error: (err) => {
        this.toast.error(getApiErrorMessage(err, 'Giriş başarısız.'));
        this.loading = false;
      }
    });
  }
}
