/** Small presentation helpers shared by pages. */

export function relativeTime(iso: string | null | undefined, locale = 'en'): string {
  if (!iso) return '—';
  const date = new Date(iso);
  const diff = (date.getTime() - Date.now()) / 1000;
  const rtf = new Intl.RelativeTimeFormat(locale, { numeric: 'auto' });
  const abs = Math.abs(diff);
  if (abs < 60) return rtf.format(Math.round(diff), 'second');
  if (abs < 3600) return rtf.format(Math.round(diff / 60), 'minute');
  if (abs < 86400) return rtf.format(Math.round(diff / 3600), 'hour');
  if (abs < 86400 * 30) return rtf.format(Math.round(diff / 86400), 'day');
  return date.toLocaleDateString(locale, { year: 'numeric', month: 'short', day: 'numeric' });
}

export function formatDateTime(iso: string | null | undefined, locale = 'en'): string {
  if (!iso) return '—';
  return new Date(iso).toLocaleString(locale, { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
}

/** "auth.login.failed" → "Login failed" */
export function humanizeEvent(eventType: string): string {
  const parts = eventType.split('.');
  const words = (parts.length > 1 ? parts.slice(1) : parts).join(' ').replace(/_/g, ' ');
  return words.charAt(0).toUpperCase() + words.slice(1);
}

export function eventIcon(eventType: string): string {
  if (eventType.startsWith('auth.login') || eventType.startsWith('auth.magic')) return 'login';
  if (eventType.startsWith('auth.logout')) return 'logout';
  if (eventType.includes('mfa')) return 'phonelink_lock';
  if (eventType.includes('password')) return 'password';
  if (eventType.includes('reuse') || eventType.includes('replay')) return 'gpp_maybe';
  if (eventType.startsWith('session')) return 'devices';
  if (eventType.startsWith('oauth')) return 'key';
  if (eventType.startsWith('user')) return 'person';
  if (eventType.startsWith('application')) return 'apps';
  if (eventType.startsWith('security')) return 'encrypted';
  return 'history';
}

/** Short, readable device description from a user-agent string. */
export function describeUserAgent(ua: string | null | undefined): { label: string; icon: string } {
  if (!ua) return { label: 'Unknown device', icon: 'devices' };
  const browser = /Edg\//.test(ua) ? 'Edge' : /Chrome\//.test(ua) ? 'Chrome' : /Firefox\//.test(ua) ? 'Firefox' : /Safari\//.test(ua) ? 'Safari' : /curl|python|okhttp|Postman/i.test(ua) ? 'API client' : 'Browser';
  const os = /Windows/.test(ua) ? 'Windows' : /Mac OS X|Macintosh/.test(ua) ? 'macOS' : /Android/.test(ua) ? 'Android' : /iPhone|iPad|iOS/.test(ua) ? 'iOS' : /Linux/.test(ua) ? 'Linux' : '';
  const mobile = /Mobile|Android|iPhone/.test(ua);
  return { label: os ? `${browser} on ${os}` : browser, icon: mobile ? 'smartphone' : browser === 'API client' ? 'terminal' : 'computer' };
}

export function initials(value: string | null | undefined): string {
  return (value || '?').split(/[\s@._-]+/).filter(Boolean).slice(0, 2).map(p => p[0]).join('').toUpperCase();
}
