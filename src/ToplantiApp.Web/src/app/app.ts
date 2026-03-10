import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
      <div class="container">
        <a class="navbar-brand" routerLink="/">Toplantı Yönetimi</a>
        @if (auth.isLoggedIn()) {
          <div class="navbar-nav ms-auto">
            <a class="nav-link" routerLink="/meetings" routerLinkActive="active">Toplantılar</a>
            <a class="nav-link" routerLink="/meetings/create" routerLinkActive="active">Yeni Toplantı</a>
            <span class="nav-link text-light">{{ auth.user()?.firstName }}</span>
            <a class="nav-link" style="cursor:pointer" (click)="auth.logout()">Çıkış</a>
          </div>
        } @else {
          <div class="navbar-nav ms-auto">
            <a class="nav-link" routerLink="/login" routerLinkActive="active">Giriş</a>
            <a class="nav-link" routerLink="/register" routerLinkActive="active">Kayıt Ol</a>
          </div>
        }
      </div>
    </nav>
    <div class="container mt-4">
      <router-outlet />
    </div>
  `
})
export class App {
  constructor(public auth: AuthService) {}
}
