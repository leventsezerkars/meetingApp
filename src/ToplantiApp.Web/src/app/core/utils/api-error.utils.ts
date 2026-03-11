import { HttpErrorResponse } from '@angular/common/http';

/** API'den donen Response yapisindaki hata detayini okur (success: false, message, statusCode). */
export function getApiErrorMessage(err: unknown, fallback = 'Bir hata oluştu.'): string {
  if (err instanceof HttpErrorResponse && err.error && typeof err.error === 'object') {
    const msg = (err.error as { message?: string }).message;
    if (typeof msg === 'string' && msg.trim()) return msg.trim();
  }
  if (err && typeof err === 'object' && 'message' in err && typeof (err as { message: unknown }).message === 'string') {
    return (err as { message: string }).message;
  }
  return fallback;
}

/** Hata response'unda statusCode varsa dondurur. */
export function getApiErrorStatusCode(err: unknown): number | undefined {
  if (err instanceof HttpErrorResponse) return err.status;
  if (err && typeof err === 'object' && 'error' in err && err.error && typeof err.error === 'object') {
    const code = (err.error as { statusCode?: number }).statusCode;
    if (typeof code === 'number') return code;
  }
  return undefined;
}
