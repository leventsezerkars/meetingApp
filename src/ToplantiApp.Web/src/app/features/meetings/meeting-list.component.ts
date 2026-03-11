import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { MeetingService } from '../../core/services/meeting.service';
import { MeetingListDto } from '../../core/models/meeting.model';
import { formatUtcToLocal } from '../../core/utils/date.utils';
import { ToastService } from '../../core/services/toast.service';
import { getApiErrorMessage } from '../../core/utils/api-error.utils';

@Component({
  selector: 'app-meeting-list',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './meeting-list.component.html'
})
export class MeetingListComponent implements OnInit {
  meetings: MeetingListDto[] = [];
  searchTerm = '';
  readonly formatUtcToLocal = formatUtcToLocal;

  constructor(
    private meetingService: MeetingService,
    private title: Title,
    private toast: ToastService
  ) {}

  isNotStarted(startDate: string): boolean {
    return new Date(startDate) > new Date();
  }

  deleteMeeting(m: MeetingListDto): void {
    if (!confirm(`"${m.name}" toplantısını kalıcı olarak silmek istiyor musunuz?`)) return;
    this.meetingService.delete(m.id).subscribe({
      next: () => {
        this.toast.success('Toplantı silindi.');
        this.meetingService.getAll().subscribe(res => this.meetings = res.data);
      },
      error: (err) => this.toast.error(getApiErrorMessage(err, 'Silme başarısız.'))
    });
  }

  ngOnInit(): void {
    this.title.setTitle('Toplantılar | Toplantı Yönetimi');
    this.meetingService.getAll().subscribe(res => this.meetings = res.data);
  }

  get filteredMeetings(): MeetingListDto[] {
    if (!this.searchTerm) return this.meetings;
    const term = this.searchTerm.toLowerCase();
    return this.meetings.filter(m =>
      m.name.toLowerCase().includes(term) || m.createdByName.toLowerCase().includes(term));
  }
}
