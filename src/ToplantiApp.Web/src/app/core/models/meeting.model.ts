export type { UserDto } from './auth.model';
import type { UserDto } from './auth.model';

export interface CreateMeetingDto {
  name: string;
  description?: string;
  startDate: string;
  endDate: string;
}

export interface UpdateMeetingDto {
  name: string;
  description?: string;
  startDate: string;
  endDate: string;
}

export interface MeetingDto {
  id: number;
  name: string;
  description: string | null;
  startDate: string;
  endDate: string;
  status: string;
  cancelledAt: string | null;
  accessToken: string;
  meetingUrl: string;
  createdBy: UserDto;
  participants: ParticipantDto[];
  documents: MeetingDocumentDto[];
  createdAt: string;
}

export interface MeetingListDto {
  id: number;
  name: string;
  startDate: string;
  endDate: string;
  status: string;
  participantCount: number;
  createdByName: string;
  createdAt: string;
}

export interface AddParticipantDto {
  userId?: number;
  email?: string;
  fullName?: string;
}

export interface ParticipantDto {
  id: number;
  userId: number | null;
  email: string;
  fullName: string;
  participantType: string;
  invitedAt: string;
}

export interface MeetingDocumentDto {
  id: number;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  isCompressed: boolean;
  uploadedAt: string;
}

export interface MeetingRoomDto {
  id: number;
  name: string;
  description: string | null;
  startDate: string;
  endDate: string;
  status: string;
  createdByName: string;
  participants: ParticipantDto[];
  documents: MeetingDocumentDto[];
}

export interface MeetingAccessResult {
  isAccessible: boolean;
  message: string | null;
  messageDate: string | null;
  messageDateLabel: string | null;
  meeting: MeetingRoomDto | null;
}
