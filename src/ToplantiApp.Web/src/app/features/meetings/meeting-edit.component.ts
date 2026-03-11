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
  templateUrl: './meeting-edit.component.html'
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
