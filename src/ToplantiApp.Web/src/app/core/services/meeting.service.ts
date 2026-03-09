import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateMeetingDto, UpdateMeetingDto, MeetingDto,
  MeetingListDto, AddParticipantDto, ParticipantDto,
  MeetingDocumentDto, MeetingAccessResult, UserDto
} from '../models/meeting.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class MeetingService {
  private readonly apiUrl = `${environment.apiUrl}/meeting`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<MeetingListDto[]> {
    return this.http.get<MeetingListDto[]>(this.apiUrl);
  }

  getById(id: number): Observable<MeetingDto> {
    return this.http.get<MeetingDto>(`${this.apiUrl}/${id}`);
  }

  create(data: CreateMeetingDto): Observable<MeetingDto> {
    return this.http.post<MeetingDto>(this.apiUrl, data);
  }

  update(id: number, data: UpdateMeetingDto): Observable<MeetingDto> {
    return this.http.put<MeetingDto>(`${this.apiUrl}/${id}`, data);
  }

  cancel(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/cancel`, {});
  }

  addParticipant(meetingId: number, data: AddParticipantDto): Observable<ParticipantDto> {
    return this.http.post<ParticipantDto>(
      `${environment.apiUrl}/meetings/${meetingId}/participants`, data);
  }

  removeParticipant(meetingId: number, participantId: number): Observable<void> {
    return this.http.delete<void>(
      `${environment.apiUrl}/meetings/${meetingId}/participants/${participantId}`);
  }

  searchUsers(term: string): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(
      `${environment.apiUrl}/meetings/0/participants/search-users`, { params: { term } });
  }

  uploadDocument(meetingId: number, file: File, compress = true): Observable<MeetingDocumentDto> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<MeetingDocumentDto>(
      `${environment.apiUrl}/meetings/${meetingId}/documents?compress=${compress}`, formData);
  }

  downloadDocument(meetingId: number, documentId: number): Observable<Blob> {
    return this.http.get(
      `${environment.apiUrl}/meetings/${meetingId}/documents/${documentId}`,
      { responseType: 'blob' });
  }

  getMeetingRoom(accessToken: string): Observable<MeetingAccessResult> {
    return this.http.get<MeetingAccessResult>(
      `${environment.apiUrl}/meeting-room/${accessToken}`);
  }
}
