import { HttpErrorResponse } from '@angular/common/http';

export const UNREACHABLE_SERVER_MESSAGE_VI =
  'Không thể kết nối tới máy chủ. Hãy kiểm tra API đã chạy và địa chỉ trong cấu hình.';

export function isUnreachableServerError(err: unknown): boolean {
  return err instanceof HttpErrorResponse && err.status === 0;
}

export function getHttpErrorMessage(
  err: unknown,
  fallback: string,
  unreachableMessage: string
): string {
  if (isUnreachableServerError(err)) {
    return unreachableMessage;
  }
  if (err instanceof HttpErrorResponse) {
    const body = err.error;
    if (body && typeof body === 'object') {
      const o = body as { message?: string; error?: { message?: string } };
      const msg = o.message ?? o.error?.message;
      if (typeof msg === 'string' && msg.trim()) {
        return msg;
      }
    }
  }
  return fallback;
}
