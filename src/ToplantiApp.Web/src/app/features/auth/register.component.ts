import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="row justify-content-center">
      <div class="col-md-6">
        <div class="card shadow">
          <div class="card-body p-4">
            <h3 class="text-center mb-4">Kayit Ol</h3>
            @if (error) {
              <div class="alert alert-danger">{{ error }}</div>
            }
            <form (ngSubmit)="onSubmit()">
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label class="form-label">Ad</label>
                  <input type="text" class="form-control" [(ngModel)]="firstName" name="firstName" required>
                </div>
                <div class="col-md-6 mb-3">
                  <label class="form-label">Soyad</label>
                  <input type="text" class="form-control" [(ngModel)]="lastName" name="lastName" required>
                </div>
              </div>
              <div class="mb-3">
                <label class="form-label">E-posta</label>
                <input type="email" class="form-control" [(ngModel)]="email" name="email" required>
              </div>
              <div class="mb-3">
                <label class="form-label">Telefon</label>
                <input type="tel" class="form-control" [(ngModel)]="phone" name="phone" required>
              </div>
              <div class="mb-3">
                <label class="form-label">Sifre</label>
                <input type="password" class="form-control" [(ngModel)]="password" name="password" required minlength="6">
              </div>
              <div class="mb-3">
                <label class="form-label">Profil Resmi</label>
                <input type="file" class="form-control" (change)="onFileChange($event)" accept="image/*">
              </div>
              <button type="submit" class="btn btn-primary w-100" [disabled]="loading">
                {{ loading ? 'Kayit yapiliyor...' : 'Kayit Ol' }}
              </button>
            </form>
            <p class="text-center mt-3">
              Zaten hesabiniz var mi? <a routerLink="/login">Giris Yap</a>
            </p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class RegisterComponent {
  firstName = '';
  lastName = '';
  email = '';
  phone = '';
  password = '';
  profileImage: File | null = null;
  error = '';
  loading = false;

  constructor(private authService: AuthService, private router: Router) {}

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.profileImage = input.files[0];
    }
  }

  onSubmit(): void {
    this.loading = true;
    this.error = '';

    const formData = new FormData();
    formData.append('firstName', this.firstName);
    formData.append('lastName', this.lastName);
    formData.append('email', this.email);
    formData.append('phone', this.phone);
    formData.append('password', this.password);
    if (this.profileImage) {
      formData.append('profileImage', this.profileImage);
    }

    this.authService.register(formData).subscribe({
      next: () => {
        this.router.navigate(['/meetings']);
      },
      error: (err) => {
        this.error = err.error?.message || 'Kayit basarisiz.';
        this.loading = false;
      }
    });
  }
}
