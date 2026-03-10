import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MeetingService } from '../../core/services/meeting.service';
import { MeetingListDto } from '../../core/models/meeting.model';

@Component({
  selector: 'app-meeting-list',
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h2>Toplantılar</h2>
      <a routerLink="/meetings/create" class="btn btn-primary">+ Yeni Toplantı</a>
    </div>

    <div class="mb-3">
      <input type="text" class="form-control" placeholder="Toplantı ara..." [(ngModel)]="searchTerm">
    </div>

    <div class="table-responsive">
      <table class="table table-hover">
        <thead class="table-light">
          <tr>
            <th>Toplantı Adı</th>
            <th>Başlangıç</th>
            <th>Bitiş</th>
            <th>Durum</th>
            <th>Katılımcı</th>
            <th>Oluşturan</th>
            <th>İşlemler</th>
          </tr>
        </thead>
        <tbody>
          @for (m of filteredMeetings; track m.id) {
            <tr>
              <td><a [routerLink]="['/meetings', m.id]">{{ m.name }}</a></td>
              <td>{{ m.startDate | date:'dd.MM.yyyy HH:mm' }}</td>
              <td>{{ m.endDate | date:'dd.MM.yyyy HH:mm' }}</td>
              <td>
                <span [class]="m.status === 'Active' ? 'badge bg-success' : 'badge bg-danger'">
                  {{ m.status === 'Active' ? 'Aktif' : 'İptal' }}
                </span>
              </td>
              <td>{{ m.participantCount }}</td>
              <td>{{ m.createdByName }}</td>
              <td>
                <a [routerLink]="['/meetings', m.id]" class="btn btn-sm btn-outline-primary me-1">Detay</a>
                @if (m.status === 'Active') {
                  <a [routerLink]="['/meetings', m.id, 'edit']" class="btn btn-sm btn-outline-secondary">Düzenle</a>
                }
              </td>
            </tr>
          }
        </tbody>
      </table>
    </div>

    @if (meetings.length === 0) {
      <div class="text-center text-muted py-5">
        <p>Henüz toplantı bulunmuyor.</p>
        <a routerLink="/meetings/create" class="btn btn-primary">İlk Toplantınızı Oluşturun</a>
      </div>
    }
  `
})
export class MeetingListComponent implements OnInit {
  meetings: MeetingListDto[] = [];
  searchTerm = '';

  constructor(private meetingService: MeetingService) {}

  get filteredMeetings(): MeetingListDto[] {
    if (!this.searchTerm) return this.meetings;
    const term = this.searchTerm.toLowerCase();
    return this.meetings.filter(m =>
      m.name.toLowerCase().includes(term) || m.createdByName.toLowerCase().includes(term));
  }

  ngOnInit(): void {
    this.meetingService.getAll().subscribe(res => this.meetings = res.data);
  }
}
