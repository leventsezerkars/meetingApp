import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MeetingService } from '../../core/services/meeting.service';
import { MeetingDto, ParticipantDto } from '../../core/models/meeting.model';
import { UserDto } from '../../core/models/auth.model';

@Component({
  selector: 'app-meeting-detail',
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    @if (meeting) {
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>{{ meeting.name }}</h2>
        <div class="d-flex gap-2">
          @if (meeting.status === 'Active') {
            <a [routerLink]="['/meetings', meeting.id, 'edit']" class="btn btn-outline-primary">Düzenle</a>
            <button class="btn btn-outline-danger" (click)="cancelMeeting()">İptal Et</button>
          }
          <a routerLink="/meetings" class="btn btn-secondary">Geri</a>
        </div>
      </div>

      <div class="row">
        <div class="col-md-8">
          <div class="card shadow mb-4">
            <div class="card-body">
              <h5 class="card-title">Toplantı Bilgileri</h5>
              <div class="row mb-2">
                <div class="col-4 fw-bold">Durum:</div>
                <div class="col-8">
                  <span [class]="meeting.status === 'Active' ? 'badge bg-success' : 'badge bg-danger'">
                    {{ meeting.status === 'Active' ? 'Aktif' : 'İptal' }}
                  </span>
                </div>
              </div>
              <div class="row mb-2">
                <div class="col-4 fw-bold">Başlangıç:</div>
                <div class="col-8">{{ meeting.startDate | date:'dd.MM.yyyy HH:mm' }}</div>
              </div>
              <div class="row mb-2">
                <div class="col-4 fw-bold">Bitiş:</div>
                <div class="col-8">{{ meeting.endDate | date:'dd.MM.yyyy HH:mm' }}</div>
              </div>
              <div class="row mb-2">
                <div class="col-4 fw-bold">Açıklama:</div>
                <div class="col-8">{{ meeting.description || '-' }}</div>
              </div>
              <div class="row mb-2">
                <div class="col-4 fw-bold">Erişim Linki:</div>
                <div class="col-8">
                  <code class="small">{{ getMeetingUrl() }}</code>
                  <button class="btn btn-sm btn-outline-secondary ms-2" (click)="copyLink()">Kopyala</button>
                </div>
              </div>
            </div>
          </div>

          <!-- Katılımcılar -->
          <div class="card shadow mb-4">
            <div class="card-body">
              <h5 class="card-title">Katılımcılar</h5>

              <!-- Dahili kullanıcı arama -->
              <div class="mb-3">
                <label class="form-label">Dahili Kullanıcı Ekle</label>
                <input type="text" class="form-control" placeholder="Kullanıcı ara..."
                       [(ngModel)]="userSearch" (input)="searchUsers()">
                @if (searchResults.length > 0) {
                  <ul class="list-group mt-1">
                    @for (u of searchResults; track u.id) {
                      <li class="list-group-item list-group-item-action d-flex justify-content-between"
                          (click)="addInternalParticipant(u)">
                        {{ u.firstName }} {{ u.lastName }} ({{ u.email }})
                        <span class="badge bg-primary">Ekle</span>
                      </li>
                    }
                  </ul>
                }
              </div>

              <!-- Harici katılımcı -->
              <div class="mb-3">
                <label class="form-label">Harici Katılımcı Ekle</label>
                <div class="row g-2">
                  <div class="col-md-5">
                    <input type="text" class="form-control" placeholder="Ad Soyad" [(ngModel)]="extName" name="extName">
                  </div>
                  <div class="col-md-5">
                    <input type="email" class="form-control" placeholder="E-posta" [(ngModel)]="extEmail" name="extEmail">
                  </div>
                  <div class="col-md-2">
                    <button class="btn btn-success w-100" (click)="addExternalParticipant()">Ekle</button>
                  </div>
                </div>
              </div>

              <table class="table table-sm">
                <thead>
                  <tr>
                    <th>Ad</th>
                    <th>E-posta</th>
                    <th>Tip</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  @for (p of meeting.participants; track p.id) {
                    <tr>
                      <td>{{ p.fullName }}</td>
                      <td>{{ p.email }}</td>
                      <td>
                        <span [class]="p.participantType === 'Internal' ? 'badge bg-info' : 'badge bg-warning'">
                          {{ p.participantType === 'Internal' ? 'Dahili' : 'Harici' }}
                        </span>
                      </td>
                      <td>
                        <button class="btn btn-sm btn-outline-danger" (click)="removeParticipant(p)">Sil</button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- Dokümanlar -->
        <div class="col-md-4">
          <div class="card shadow">
            <div class="card-body">
              <h5 class="card-title">Dokümanlar</h5>
              <div class="mb-3">
                <input type="file" class="form-control" (change)="onFileSelect($event)">
                @if (selectedFile) {
                  <button class="btn btn-primary btn-sm mt-2" (click)="uploadDocument()">Yükle</button>
                }
              </div>
              @for (doc of meeting.documents; track doc.id) {
                <div class="d-flex justify-content-between align-items-center mb-2">
                  <span class="small">{{ doc.originalFileName }}</span>
                  <button class="btn btn-sm btn-outline-primary" (click)="downloadDoc(doc.id)">İndir</button>
                </div>
              }
              @if (meeting.documents.length === 0) {
                <p class="text-muted small">Henüz doküman yok.</p>
              }
            </div>
          </div>
        </div>
      </div>
    }
  `
})
export class MeetingDetailComponent implements OnInit {
  meeting: MeetingDto | null = null;
  userSearch = '';
  searchResults: UserDto[] = [];
  extName = '';
  extEmail = '';
  selectedFile: File | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private meetingService: MeetingService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.loadMeeting(id);
  }

  loadMeeting(id: number): void {
    this.meetingService.getById(id).subscribe(res => this.meeting = res.data);
  }

  getMeetingUrl(): string {
    return `${window.location.origin}/meeting-room/${this.meeting?.accessToken}`;
  }

  copyLink(): void {
    navigator.clipboard.writeText(this.getMeetingUrl());
  }

  searchUsers(): void {
    if (this.userSearch.length < 2) { this.searchResults = []; return; }
    this.meetingService.searchUsers(this.userSearch).subscribe(res => this.searchResults = res.data);
  }

  addInternalParticipant(user: UserDto): void {
    this.meetingService.addParticipant(this.meeting!.id, { userId: user.id }).subscribe({
      next: () => { this.loadMeeting(this.meeting!.id); this.searchResults = []; this.userSearch = ''; },
      error: (err) => alert(err.error?.message || 'Hata')
    });
  }

  addExternalParticipant(): void {
    if (!this.extName || !this.extEmail) return;
    this.meetingService.addParticipant(this.meeting!.id, {
      email: this.extEmail, fullName: this.extName
    }).subscribe({
      next: () => { this.loadMeeting(this.meeting!.id); this.extName = ''; this.extEmail = ''; },
      error: (err) => alert(err.error?.message || 'Hata')
    });
  }

  removeParticipant(p: ParticipantDto): void {
    if (!confirm(`${p.fullName} katılımcısını silmek istiyor musunuz?`)) return;
    this.meetingService.removeParticipant(this.meeting!.id, p.id)
      .subscribe(() => this.loadMeeting(this.meeting!.id));
  }

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  uploadDocument(): void {
    if (!this.selectedFile || !this.meeting) return;
    this.meetingService.uploadDocument(this.meeting.id, this.selectedFile).subscribe({
      next: () => { this.loadMeeting(this.meeting!.id); this.selectedFile = null; },
      error: (err) => alert(err.error?.message || 'Yükleme başarısız.')
    });
  }

  downloadDoc(docId: number): void {
    this.meetingService.downloadDocument(this.meeting!.id, docId).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = '';
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }

  cancelMeeting(): void {
    if (!confirm('Bu toplantıyı iptal etmek istiyor musunuz?')) return;
    this.meetingService.cancel(this.meeting!.id).subscribe({
      next: () => this.loadMeeting(this.meeting!.id),
      error: (err) => alert(err.error?.message || 'İptal başarısız.')
    });
  }
}
