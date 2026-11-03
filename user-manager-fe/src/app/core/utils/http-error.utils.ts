import { HttpErrorResponse } from '@angular/common/http';

export const UNREACHABLE_SERVER_MESSAGE =
  'Cannot reach the server. Check that the API is running and the configured URL is correct.';
/** @deprecated kept for existing imports; the message was double-encoded in the original source. */
export const UNREACHABLE_SERVER_MESSAGE_VI = UNREACHABLE_SERVER_MESSAGE;

const GENERIC_SERVER_MESSAGES = new Set([
  'internal server error',
  'unauthorized',
  'bad request',
  'business rule violation',
  'validation failed',
  'authentication failed',
  'operation not allowed',
  'resource not found'
]);

type ApiErrorPayload = {
  message?: string;
  detail?: string;
  title?: string;
  errors?: Record<string, string[]>;
  error?: {
    message?: string;
    code?: string;
    errors?: Record<string, string[]>;
  };
};

export function isUnreachableServerError(err: unknown): boolean {
  return err instanceof HttpErrorResponse && err.status === 0;
}

function isMeaningfulMessage(value: unknown): value is string {
  return typeof value === 'string' && value.trim().length > 0;
}

function isGenericServerMessage(value: string): boolean {
  return GENERIC_SERVER_MESSAGES.has(value.trim().toLowerCase());
}

function flattenValidationErrors(errors: unknown): string | null {
  if (!errors || typeof errors !== 'object') {
    return null;
  }

  const parts = Object.values(errors as Record<string, unknown>)
    .flatMap((entry) => Array.isArray(entry) ? entry : [])
    .filter((entry): entry is string => typeof entry === 'string' && entry.trim().length > 0);

  return parts.length ? parts.join(' ') : null;
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

    if (typeof body === 'string' && body.trim()) {
      return body.trim();
    }

    if (body && typeof body === 'object') {
      const payload = body as ApiErrorPayload;

      const detailedMessage =
        (isMeaningfulMessage(payload.error?.message) ? payload.error.message.trim() : null) ??
        (isMeaningfulMessage(payload.detail) ? payload.detail.trim() : null) ??
        flattenValidationErrors(payload.error?.errors) ??
        flattenValidationErrors(payload.errors);

      if (detailedMessage) {
        return detailedMessage;
      }

      if (isMeaningfulMessage(payload.message) && !isGenericServerMessage(payload.message)) {
        return payload.message.trim();
      }

      if (isMeaningfulMessage(payload.title) && !isGenericServerMessage(payload.title)) {
        return payload.title.trim();
      }
    }

    if (isMeaningfulMessage(err.statusText) && !isGenericServerMessage(err.statusText)) {
      return err.statusText.trim();
    }
  }

  return fallback;
}

/** Convenience wrapper with the default "server unreachable" message. */
export function apiErrorMessage(err: unknown, fallback = 'Something went wrong. Please try again.'): string {
  return getHttpErrorMessage(err, fallback, UNREACHABLE_SERVER_MESSAGE);
}

/** Stable error code from the API envelope (e.g. INVALID_CREDENTIALS, MFA_REQUIRED). */
export function apiErrorCode(err: unknown): string | null {
  if (err instanceof HttpErrorResponse && err.error && typeof err.error === 'object') {
    const code = (err.error as ApiErrorPayload).error?.code;
    return typeof code === 'string' ? code : null;
  }
  return null;
}
