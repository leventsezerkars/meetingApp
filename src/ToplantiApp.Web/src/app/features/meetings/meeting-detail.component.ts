import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { MeetingService } from '../../core/services/meeting.service';
import { MeetingDto, ParticipantDto } from '../../core/models/meeting.model';
import { UserDto } from '../../core/models/auth.model';
import { formatUtcToLocal, isMeetingNotStarted } from '../../core/utils/date.utils';
import { getApiErrorMessage } from '../../core/utils/api-error.utils';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-meeting-detail',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './meeting-detail.component.html'
})
export class MeetingDetailComponent implements OnInit {
  meeting: MeetingDto | null = null;
  userSearch = '';
  searchResults: UserDto[] = [];
  extName = '';
  extEmail = '';
  selectedFile: File | null = null;
  showCancelConfirm = false;
  showDeleteConfirm = false;
  readonly formatUtcToLocal = formatUtcToLocal;
  readonly isMeetingNotStarted = isMeetingNotStarted;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private meetingService: MeetingService,
    private toast: ToastService,
    private title: Title
  ) {}

  ngOnInit(): void {
    this.title.setTitle('Toplantı Detayı | Toplantı Yönetimi');
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.loadMeeting(id);
  }

  loadMeeting(id: number): void {
    this.meetingService.getById(id).subscribe(res => {
      this.meeting = res.data;
      if (this.meeting) this.title.setTitle(this.meeting.name + ' | Toplantı Yönetimi');
    });
  }

  getMeetingUrl(): string {
    return `${window.location.origin}/meeting-room/${this.meeting?.accessToken}`;
  }

  copyLink(): void {
    navigator.clipboard.writeText(this.getMeetingUrl());
    this.toast.success('Erişim linki kopyalandı.');
  }

  searchUsers(): void {
    if (this.userSearch.length < 2) { this.searchResults = []; return; }
    this.meetingService.searchUsers(this.userSearch).subscribe(res => this.searchResults = res.data);
  }

  addInternalParticipant(user: UserDto): void {
    this.meetingService.addParticipant(this.meeting!.id, { userId: user.id }).subscribe({
      next: () => {
        this.toast.success('Katılımcı eklendi.');
        this.loadMeeting(this.meeting!.id);
        this.searchResults = [];
        this.userSearch = '';
      },
      error: (err) => this.toast.error(getApiErrorMessage(err, 'Hata'))
    });
  }

  addExternalParticipant(): void {
    if (!this.extName || !this.extEmail) return;
    this.meetingService.addParticipant(this.meeting!.id, {
      email: this.extEmail, fullName: this.extName
    }).subscribe({
      next: () => {
        this.toast.success('Katılımcı eklendi.');
        this.loadMeeting(this.meeting!.id);
        this.extName = '';
        this.extEmail = '';
      },
      error: (err) => this.toast.error(getApiErrorMessage(err, 'Hata'))
    });
  }

  removeParticipant(p: ParticipantDto): void {
    if (!confirm(`${p.fullName} katılımcısını silmek istiyor musunuz?`)) return;
    this.meetingService.removeParticipant(this.meeting!.id, p.id).subscribe({
      next: () => {
        this.toast.success('Katılımcı kaldırıldı.');
        this.loadMeeting(this.meeting!.id);
      },
      error: (err) => this.toast.error(getApiErrorMessage(err, 'Hata'))
    });
  }

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  uploadDocument(): void {
    if (!this.selectedFile || !this.meeting) return;
    this.meetingService.uploadDocument(this.meeting.id, this.selectedFile).subscribe({
      next: () => {
        this.toast.success('Doküman yüklendi.');
        this.loadMeeting(this.meeting!.id);
        this.selectedFile = null;
      },
      error: (err) => this.toast.error(getApiErrorMessage(err, 'Yükleme başarısız.'))
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

  deleteMeeting(): void {
    this.showDeleteConfirm = true;
  }

  closeDeleteConfirm(): void {
    this.showDeleteConfirm = false;
  }

  confirmDelete(): void {
    if (!this.meeting) return;
    this.meetingService.delete(this.meeting.id).subscribe({
      next: () => {
        this.showDeleteConfirm = false;
        this.toast.success('Toplantı silindi.');
        this.router.navigate(['/meetings']);
      },
      error: (err) => {
        this.showDeleteConfirm = false;
        this.toast.error(getApiErrorMessage(err, 'Silme başarısız.'));
      }
    });
  }

  cancelMeeting(): void {
    this.showCancelConfirm = true;
  }

  closeCancelConfirm(): void {
    this.showCancelConfirm = false;
  }

  confirmCancel(): void {
    if (!this.meeting) return;
    this.meetingService.cancel(this.meeting.id).subscribe({
      next: () => {
        this.showCancelConfirm = false;
        this.toast.success('Toplantı iptal edildi.');
        this.loadMeeting(this.meeting!.id);
      },
      error: (err) => this.toast.error(getApiErrorMessage(err, 'İptal başarısız.'))
    });
  }
}
