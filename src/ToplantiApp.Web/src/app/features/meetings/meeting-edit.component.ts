import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { MeetingService } from '../../core/services/meeting.service';
import { ToastService } from '../../core/services/toast.service';
import { getApiErrorMessage } from '../../core/utils/api-error.utils';

@Component({
  selector: 'app-meeting-edit',
  imports: [CommonModule, FormsModule],
  template: `
    <h2 class="mb-4">Toplantı Düzenle</h2>
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
          {{ loading ? 'Kaydediliyor...' : 'Kaydet' }}
        </button>
        <button type="button" class="btn btn-secondary" (click)="router.navigate(['/meetings', meetingId])">İptal</button>
      </div>
    </form>
  `
})
export class MeetingEditComponent implements OnInit {
  meetingId = 0;
  name = '';
  description = '';
  startDate = '';
  endDate = '';
  loading = false;

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private meetingService: MeetingService,
    private toast: ToastService,
    private title: Title
  ) {}

  ngOnInit(): void {
    this.title.setTitle('Toplantı Düzenle | Toplantı Yönetimi');
    this.meetingId = Number(this.route.snapshot.paramMap.get('id'));
    this.meetingService.getById(this.meetingId).subscribe(res => {
      const m = res.data;
      this.name = m.name;
      this.description = m.description || '';
      this.startDate = this.toLocalDatetime(m.startDate);
      this.endDate = this.toLocalDatetime(m.endDate);
    });
  }

  /** API'den gelen UTC ISO string'i, datetime-local input icin yerel saate cevirir. */
  private toLocalDatetime(iso: string): string {
    const d = new Date(iso);
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const h = String(d.getHours()).padStart(2, '0');
    const min = String(d.getMinutes()).padStart(2, '0');
    return `${y}-${m}-${day}T${h}:${min}`;
  }

  onSubmit(): void {
    this.loading = true;
    this.meetingService.update(this.meetingId, {
      name: this.name,
      description: this.description || undefined,
      startDate: new Date(this.startDate).toISOString(),
      endDate: new Date(this.endDate).toISOString()
    }).subscribe({
      next: () => {
        this.toast.success('Toplantı güncellendi.');
        this.router.navigate(['/meetings', this.meetingId]);
      },
      error: (err) => {
        this.toast.error(getApiErrorMessage(err, 'Güncelleme başarısız.'));
        this.loading = false;
      }
    });
  }
}
