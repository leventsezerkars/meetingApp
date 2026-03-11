import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { MeetingService } from '../../core/services/meeting.service';
import { MeetingAccessResult } from '../../core/models/meeting.model';
import { formatUtcToLocal } from '../../core/utils/date.utils';
import { getApiErrorMessage } from '../../core/utils/api-error.utils';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-meeting-room',
  imports: [CommonModule],
  templateUrl: './meeting-room.component.html'
})
export class MeetingRoomComponent implements OnInit {
  result: MeetingAccessResult | null = null;
  loading = true;
  errorMsg = '';
  readonly formatUtcToLocal = formatUtcToLocal;

  constructor(
    private route: ActivatedRoute,
    private meetingService: MeetingService,
    private toast: ToastService,
    private title: Title
  ) {}

  ngOnInit(): void {
    this.title.setTitle('Toplantı Odası | Toplantı Yönetimi');
    const token = this.route.snapshot.paramMap.get('accessToken')!;
    this.meetingService.getMeetingRoom(token).subscribe({
      next: (res) => { this.result = res.data; this.loading = false; },
      error: (err) => {
        if (err.error?.data) {
          this.result = err.error.data;
        } else {
          this.errorMsg = getApiErrorMessage(err, 'Toplantı bulunamadı veya bir hata oluştu.');
          this.toast.error(this.errorMsg);
        }
        this.loading = false;
      }
    });
  }

  downloadDoc(docId: number): void {
    if (!this.result?.meeting) return;
    this.meetingService.downloadDocument(this.result.meeting.id, docId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = '';
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => this.toast.error(getApiErrorMessage(err, 'İndirme başarısız.'))
    });
  }
}
