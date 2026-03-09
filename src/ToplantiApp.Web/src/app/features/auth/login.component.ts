import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="row justify-content-center">
      <div class="col-md-5">
        <div class="card shadow">
          <div class="card-body p-4">
            <h3 class="text-center mb-4">Giris Yap</h3>
            @if (error) {
              <div class="alert alert-danger">{{ error }}</div>
            }
            <form (ngSubmit)="onSubmit()">
              <div class="mb-3">
                <label class="form-label">E-posta</label>
                <input type="email" class="form-control" [(ngModel)]="email" name="email" required>
              </div>
              <div class="mb-3">
                <label class="form-label">Sifre</label>
                <input type="password" class="form-control" [(ngModel)]="password" name="password" required>
              </div>
              <button type="submit" class="btn btn-primary w-100" [disabled]="loading">
                {{ loading ? 'Giris yapiliyor...' : 'Giris Yap' }}
              </button>
            </form>
            <p class="text-center mt-3">
              Hesabiniz yok mu? <a routerLink="/register">Kayit Ol</a>
            </p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';
  loading = false;

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit(): void {
    this.loading = true;
    this.error = '';
    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: () => {
        this.router.navigate(['/meetings']);
      },
      error: (err) => {
        this.error = err.error?.error || 'Giris basarisiz.';
        this.loading = false;
      }
    });
  }
}
