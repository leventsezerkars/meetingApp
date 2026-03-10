import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MeetingService } from '../../core/services/meeting.service';

@Component({
  selector: 'app-meeting-create',
  imports: [CommonModule, FormsModule],
  template: `
    <h2 class="mb-4">Yeni Toplanti Olustur</h2>
    @if (error) {
      <div class="alert alert-danger">{{ error }}</div>
    }
    <form (ngSubmit)="onSubmit()" class="card shadow p-4">
      <div class="mb-3">
        <label class="form-label">Toplanti Adi</label>
        <input type="text" class="form-control" [(ngModel)]="name" name="name" required>
      </div>
      <div class="mb-3">
        <label class="form-label">Aciklama</label>
        <textarea class="form-control" [(ngModel)]="description" name="description" rows="3"></textarea>
      </div>
      <div class="row">
        <div class="col-md-6 mb-3">
          <label class="form-label">Baslangic Tarihi</label>
          <input type="datetime-local" class="form-control" [(ngModel)]="startDate" name="startDate" required>
        </div>
        <div class="col-md-6 mb-3">
          <label class="form-label">Bitis Tarihi</label>
          <input type="datetime-local" class="form-control" [(ngModel)]="endDate" name="endDate" required>
        </div>
      </div>
      <div class="d-flex gap-2">
        <button type="submit" class="btn btn-primary" [disabled]="loading">
          {{ loading ? 'Olusturuluyor...' : 'Olustur' }}
        </button>
        <button type="button" class="btn btn-secondary" (click)="router.navigate(['/meetings'])">Iptal</button>
      </div>
    </form>
  `
})
export class MeetingCreateComponent {
  name = '';
  description = '';
  startDate = '';
  endDate = '';
  error = '';
  loading = false;

  constructor(private meetingService: MeetingService, public router: Router) {}

  onSubmit(): void {
    this.loading = true;
    this.error = '';
    this.meetingService.create({
      name: this.name,
      description: this.description || undefined,
      startDate: new Date(this.startDate).toISOString(),
      endDate: new Date(this.endDate).toISOString()
    }).subscribe({
      next: (res) => this.router.navigate(['/meetings', res.data.id]),
      error: (err) => {
        this.error = err.error?.message || 'Toplanti olusturulamadi.';
        this.loading = false;
      }
    });
  }
}
