import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { MeetingService } from '../../core/services/meeting.service';
import { ToastService } from '../../core/services/toast.service';
import { getApiErrorMessage } from '../../core/utils/api-error.utils';

@Component({
  selector: 'app-meeting-create',
  imports: [CommonModule, FormsModule],
  template: `
    <h2 class="mb-4">Yeni Toplantı Oluştur</h2>
    <form (ngSubmit)="onSubmit()" class="card shadow p-4">
      <div class="mb-3">
        <label class="form-label">Toplantı Adı</label>
        <input type="text" class="form-control" [(ngModel)]="name" name="name" required>
      </div>
      <div class="mb-3">
        <label class="form-label">Açıklama</label>
        <textarea class="form-control" [(ngModel)]="description" name="description" rows="3"></textarea>
      </div>
      <div class="row">
        <div class="col-md-6 mb-3">
          <label class="form-label">Başlangıç Tarihi</label>
          <input type="datetime-local" class="form-control" [(ngModel)]="startDate" name="startDate" required>
        </div>
        <div class="col-md-6 mb-3">
          <label class="form-label">Bitiş Tarihi</label>
          <input type="datetime-local" class="form-control" [(ngModel)]="endDate" name="endDate" required>
        </div>
      </div>
      <div class="d-flex gap-2">
        <button type="submit" class="btn btn-primary" [disabled]="loading">
          {{ loading ? 'Oluşturuluyor...' : 'Oluştur' }}
        </button>
        <button type="button" class="btn btn-secondary" (click)="router.navigate(['/meetings'])">İptal</button>
      </div>
    </form>
  `
})
export class MeetingCreateComponent implements OnInit {
  name = '';
  description = '';
  startDate = '';
  endDate = '';
  loading = false;

  constructor(
    private meetingService: MeetingService,
    private title: Title,
    public router: Router,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.title.setTitle('Yeni Toplantı | Toplantı Yönetimi');
  }

  onSubmit(): void {
    this.loading = true;
    this.meetingService.create({
      name: this.name,
      description: this.description || undefined,
      startDate: new Date(this.startDate).toISOString(),
      endDate: new Date(this.endDate).toISOString()
    }).subscribe({
      next: (res) => {
        this.toast.success('Toplantı oluşturuldu.');
        this.router.navigate(['/meetings', res.data.id]);
      },
      error: (err) => {
        this.toast.error(getApiErrorMessage(err, 'Toplantı oluşturulamadı.'));
        this.loading = false;
      }
    });
  }
}
