import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
      <div class="container">
        <a class="navbar-brand" routerLink="/">Toplanti Yonetimi</a>
        @if (auth.isLoggedIn()) {
          <div class="navbar-nav ms-auto">
            <a class="nav-link" routerLink="/meetings" routerLinkActive="active">Toplantilar</a>
            <a class="nav-link" routerLink="/meetings/create" routerLinkActive="active">Yeni Toplanti</a>
            <span class="nav-link text-light">{{ auth.user()?.firstName }}</span>
            <a class="nav-link" style="cursor:pointer" (click)="auth.logout()">Cikis</a>
          </div>
        } @else {
          <div class="navbar-nav ms-auto">
            <a class="nav-link" routerLink="/login" routerLinkActive="active">Giris</a>
            <a class="nav-link" routerLink="/register" routerLinkActive="active">Kayit Ol</a>
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
