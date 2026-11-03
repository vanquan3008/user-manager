import { Injectable, signal } from '@angular/core';

export type NotificationType = 'success' | 'error' | 'info' | 'warning' | 'validation';

export interface Notification {
  id: number;
  type: NotificationType;
  message: string;
  leaving?: boolean;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private _list = signal<Notification[]>([]);
  readonly list = this._list.asReadonly();

  private nextId = 1;

  show(type: NotificationType, message: string, durationMs?: number) {
    durationMs ??= type === 'error' || type === 'validation' ? 6000 : 3500;
    const note: Notification = { id: this.nextId++, type, message };
    this._list.update(list => [...list, note]);
    setTimeout(() => {
      this.dismiss(note.id);
    }, durationMs);
  }

  dismiss(id: number) {
    this._list.update(list => list.map(note => note.id === id ? { ...note, leaving: true } : note));
    setTimeout(() => {
      this._list.update(list => list.filter(note => note.id !== id));
    }, 220);
  }

  success(message: string, durationMs?: number) {
    this.show('success', message, durationMs);
  }

  error(message: string, durationMs?: number) {
    this.show('error', message, durationMs);
  }

  info(message: string, durationMs?: number) {
    this.show('info', message, durationMs);
  }

  validation(message: string, durationMs?: number) {
    this.show('validation', message, durationMs);
  }

  warning(message: string, durationMs?: number) {
    this.show('warning', message, durationMs);
  }
}

