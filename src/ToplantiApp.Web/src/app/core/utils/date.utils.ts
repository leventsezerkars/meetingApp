import moment from 'moment';

/**
 * API'den gelen UTC tarihi (ISO string) moment ile UTC olarak parse edip
 * kullanici yerel saatine cevirir; dd.MM.yyyy HH:mm formatinda dondurur.
 */
export function formatUtcToLocal(iso: string | null | undefined): string {
  if (iso == null || iso === '') return '';
  const trimmed = iso.trim();
  const m = moment.utc(trimmed);
  const valid = m.isValid();
  const local = m.local();
  const result = valid ? local.format('DD.MM.YYYY HH:mm') : '';
  return result;
}
