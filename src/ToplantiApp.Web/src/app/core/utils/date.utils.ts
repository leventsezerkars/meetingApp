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

/**
 * API'den gelen baslangic tarihi (UTC ISO string) ile su anki zamani karsilastirir.
 * DB GMT+0 gonderiyor; new Date(iso) bazen yerel saat gibi yorumlanabildigi icin
 * her zaman UTC parse (moment.utc) kullanilir. Boylece Sil/Iptal kosulu dogru calisir.
 * @returns true = toplanti henuz baslamadi (baslangic suandan sonra)
 */
export function isMeetingNotStarted(startDate: string | null | undefined): boolean {
  if (startDate == null || startDate === '') return false;
  const start = moment.utc(startDate.trim());
  if (!start.isValid()) return false;
  return start.isAfter(moment());
}
