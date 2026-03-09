import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/meetings', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'meetings',
    canActivate: [authGuard],
    loadComponent: () => import('./features/meetings/meeting-list.component').then(m => m.MeetingListComponent)
  },
  {
    path: 'meetings/create',
    canActivate: [authGuard],
    loadComponent: () => import('./features/meetings/meeting-create.component').then(m => m.MeetingCreateComponent)
  },
  {
    path: 'meetings/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/meetings/meeting-detail.component').then(m => m.MeetingDetailComponent)
  },
  {
    path: 'meetings/:id/edit',
    canActivate: [authGuard],
    loadComponent: () => import('./features/meetings/meeting-edit.component').then(m => m.MeetingEditComponent)
  },
  {
    path: 'meeting-room/:accessToken',
    loadComponent: () => import('./features/meeting-room/meeting-room.component').then(m => m.MeetingRoomComponent)
  },
  { path: '**', redirectTo: '/meetings' }
];
