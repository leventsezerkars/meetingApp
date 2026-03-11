import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { getApiErrorMessage } from '../../core/utils/api-error.utils';

@Component({
  selector: 'app-register',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html'
})
export class RegisterComponent implements OnInit {
  firstName = '';
  lastName = '';
  email = '';
  phone = '';
  password = '';
  profileImage: File | null = null;
  loading = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private toast: ToastService,
    private title: Title
  ) {}

  ngOnInit(): void {
    this.title.setTitle('Kayıt Ol | Toplantı Yönetimi');
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.profileImage = input.files[0];
    }
  }

  onSubmit(): void {
    this.loading = true;

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
        this.toast.success('Kayıt başarılı.');
        this.router.navigate(['/meetings']);
      },
      error: (err) => {
        this.toast.error(getApiErrorMessage(err, 'Kayıt başarısız.'));
        this.loading = false;
      }
    });
  }
}
