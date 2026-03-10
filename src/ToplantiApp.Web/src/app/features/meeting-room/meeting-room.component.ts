import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MeetingService } from '../../core/services/meeting.service';
import { MeetingAccessResult } from '../../core/models/meeting.model';

@Component({
  selector: 'app-meeting-room',
  imports: [CommonModule],
  template: `
    @if (loading) {
      <div class="text-center py-5">
        <div class="spinner-border text-primary" role="status"></div>
        <p class="mt-2">Toplantı bilgileri yükleniyor...</p>
      </div>
    }

    @if (!loading && result) {
      @if (!result.isAccessible) {
        <div class="row justify-content-center mt-5">
          <div class="col-md-6">
            <div class="card shadow border-danger">
              <div class="card-body text-center p-5">
                <h1 class="display-1 text-danger">⛔</h1>
                <h3 class="text-danger">Erişim Engellendi</h3>
                <p class="text-muted mt-3">{{ result.message }}</p>
              </div>
            </div>
          </div>
        </div>
      } @else if (result.meeting) {
        <div class="card shadow">
          <div class="card-header bg-success text-white">
            <h3 class="mb-0">{{ result.meeting.name }}</h3>
            <small>Organizatör: {{ result.meeting.createdByName }}</small>
          </div>
          <div class="card-body">
            <div class="row mb-4">
              <div class="col-md-6">
                <p><strong>Başlangıç:</strong> {{ result.meeting.startDate | date:'dd.MM.yyyy HH:mm' }}</p>
                <p><strong>Bitiş:</strong> {{ result.meeting.endDate | date:'dd.MM.yyyy HH:mm' }}</p>
              </div>
              <div class="col-md-6">
                <p><strong>Açıklama:</strong> {{ result.meeting.description || '-' }}</p>
                <p><strong>Durum:</strong> <span class="badge bg-success">Devam Ediyor</span></p>
              </div>
            </div>

            <h5>Katılımcılar</h5>
            <ul class="list-group mb-4">
              @for (p of result.meeting.participants; track p.id) {
                <li class="list-group-item d-flex justify-content-between">
                  <span>{{ p.fullName }}</span>
                  <span class="text-muted">{{ p.email }}</span>
                </li>
              }
            </ul>

            @if (result.meeting.documents.length > 0) {
              <h5>Dokümanlar</h5>
              <ul class="list-group">
                @for (doc of result.meeting.documents; track doc.id) {
                  <li class="list-group-item">{{ doc.originalFileName }}</li>
                }
              </ul>
            }
          </div>
        </div>
      }
    }

    @if (!loading && errorMsg) {
      <div class="row justify-content-center mt-5">
        <div class="col-md-6">
          <div class="alert alert-danger text-center">{{ errorMsg }}</div>
        </div>
      </div>
    }
  `
})
export class MeetingRoomComponent implements OnInit {
  result: MeetingAccessResult | null = null;
  loading = true;
  errorMsg = '';

  constructor(private route: ActivatedRoute, private meetingService: MeetingService) {}

  ngOnInit(): void {
    const token = this.route.snapshot.paramMap.get('accessToken')!;
    this.meetingService.getMeetingRoom(token).subscribe({
      next: (res) => { this.result = res.data; this.loading = false; },
      error: (err) => {
        if (err.error?.data) {
          this.result = err.error.data;
        } else {
          this.errorMsg = err.error?.message || 'Toplantı bulunamadı veya bir hata oluştu.';
        }
        this.loading = false;
      }
    });
  }
}
