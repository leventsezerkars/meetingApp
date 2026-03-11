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
  templateUrl: './meeting-create.component.html'
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
