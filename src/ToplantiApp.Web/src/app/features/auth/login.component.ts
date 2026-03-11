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
  templateUrl: './login.component.html'
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
