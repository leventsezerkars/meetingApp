import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MeetingService } from '../../core/services/meeting.service';

@Component({
  selector: 'app-meeting-edit',
  imports: [CommonModule, FormsModule],
  template: `
    <h2 class="mb-4">Toplanti Duzenle</h2>
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
          {{ loading ? 'Kaydediliyor...' : 'Kaydet' }}
        </button>
        <button type="button" class="btn btn-secondary" (click)="router.navigate(['/meetings', meetingId])">Iptal</button>
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
  error = '';
  loading = false;

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private meetingService: MeetingService
  ) {}

  ngOnInit(): void {
    this.meetingId = Number(this.route.snapshot.paramMap.get('id'));
    this.meetingService.getById(this.meetingId).subscribe(m => {
      this.name = m.name;
      this.description = m.description || '';
      this.startDate = this.toLocalDatetime(m.startDate);
      this.endDate = this.toLocalDatetime(m.endDate);
    });
  }

  private toLocalDatetime(iso: string): string {
    const d = new Date(iso);
    return d.toISOString().slice(0, 16);
  }

  onSubmit(): void {
    this.loading = true;
    this.error = '';
    this.meetingService.update(this.meetingId, {
      name: this.name,
      description: this.description || undefined,
      startDate: new Date(this.startDate).toISOString(),
      endDate: new Date(this.endDate).toISOString()
    }).subscribe({
      next: () => this.router.navigate(['/meetings', this.meetingId]),
      error: (err) => {
        this.error = err.error?.error || 'Guncelleme basarisiz.';
        this.loading = false;
      }
    });
  }
}
