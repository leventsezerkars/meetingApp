import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateMeetingDto, UpdateMeetingDto, MeetingDto,
  MeetingListDto, AddParticipantDto, ParticipantDto,
  MeetingDocumentDto, MeetingAccessResult, UserDto
} from '../models/meeting.model';
import { ApiResponse, PaginatedResponse } from '../models/response.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class MeetingService {
  private readonly apiUrl = `${environment.apiUrl}/meeting`;

  constructor(private http: HttpClient) {}

  getAll(pageNumber = 1, pageSize = 10): Observable<PaginatedResponse<MeetingListDto>> {
    return this.http.get<PaginatedResponse<MeetingListDto>>(this.apiUrl, {
      params: { pageNumber, pageSize }
    });
  }

  getById(id: number): Observable<ApiResponse<MeetingDto>> {
    return this.http.get<ApiResponse<MeetingDto>>(`${this.apiUrl}/${id}`);
  }

  create(data: CreateMeetingDto): Observable<ApiResponse<MeetingDto>> {
    return this.http.post<ApiResponse<MeetingDto>>(this.apiUrl, data);
  }

  update(id: number, data: UpdateMeetingDto): Observable<ApiResponse<MeetingDto>> {
    return this.http.put<ApiResponse<MeetingDto>>(`${this.apiUrl}/${id}`, data);
  }

  cancel(id: number): Observable<ApiResponse> {
    return this.http.put<ApiResponse>(`${this.apiUrl}/${id}/cancel`, {});
  }

  addParticipant(meetingId: number, data: AddParticipantDto): Observable<ApiResponse<ParticipantDto>> {
    return this.http.post<ApiResponse<ParticipantDto>>(
      `${environment.apiUrl}/meetings/${meetingId}/participants`, data);
  }

  removeParticipant(meetingId: number, participantId: number): Observable<ApiResponse> {
    return this.http.delete<ApiResponse>(
      `${environment.apiUrl}/meetings/${meetingId}/participants/${participantId}`);
  }

  searchUsers(term: string): Observable<ApiResponse<UserDto[]>> {
    return this.http.get<ApiResponse<UserDto[]>>(
      `${environment.apiUrl}/meetings/0/participants/search-users`, { params: { term } });
  }

  uploadDocument(meetingId: number, file: File, compress = true): Observable<ApiResponse<MeetingDocumentDto>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<MeetingDocumentDto>>(
      `${environment.apiUrl}/meetings/${meetingId}/documents?compress=${compress}`, formData);
  }

  downloadDocument(meetingId: number, documentId: number): Observable<Blob> {
    return this.http.get(
      `${environment.apiUrl}/meetings/${meetingId}/documents/${documentId}`,
      { responseType: 'blob' });
  }

  getMeetingRoom(accessToken: string): Observable<ApiResponse<MeetingAccessResult>> {
    return this.http.get<ApiResponse<MeetingAccessResult>>(
      `${environment.apiUrl}/meeting-room/${accessToken}`);
  }
}
