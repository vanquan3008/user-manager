import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class SessionStorageService {
  private readonly storagePrefix = 'user_admin.';

  get(key: string): string | null {
    return this.getStorage()?.getItem(this.toStorageKey(key)) ?? null;
  }

  set(key: string, value: string): void {
    this.getStorage()?.setItem(this.toStorageKey(key), value);
  }

  remove(key: string): void {
    this.getStorage()?.removeItem(this.toStorageKey(key));
  }

  getJson<T>(key: string, fallback: T): T {
    const raw = this.get(key);
    if (!raw) {
      return fallback;
    }

    try {
      return JSON.parse(raw) as T;
    } catch {
      return fallback;
    }
  }

  setJson(key: string, value: unknown): void {
    this.set(key, JSON.stringify(value));
  }

  removeMany(keys: string[]): void {
    for (const key of keys) {
      this.remove(key);
    }
  }

  private getStorage(): Storage | null {
    if (typeof window === 'undefined') {
      return null;
    }

    return window.sessionStorage;
  }

  private toStorageKey(key: string): string {
    return `${this.storagePrefix}${key}`;
  }
}
