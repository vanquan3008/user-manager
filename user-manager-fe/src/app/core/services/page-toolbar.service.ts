import { Injectable, signal } from '@angular/core';

/** Optional badge, description, and title overrides for the app header toolbar row. */
@Injectable({ providedIn: 'root' })
export class PageToolbarService {
  readonly badge = signal<string | null>(null);
  readonly description = signal<string | null>(null);
  /** When set, replaces layout-derived page title (e.g. detail pages). */
  readonly titleOverride = signal<string | null>(null);
  /** When set, replaces layout-derived eyebrow. */
  readonly eyebrowOverride = signal<string | null>(null);

  setMeta(meta: Partial<{ badge: string | null; description: string | null; title: string | null; eyebrow: string | null }>) {
    if (meta.badge !== undefined) this.badge.set(meta.badge);
    if (meta.description !== undefined) this.description.set(meta.description);
    if (meta.title !== undefined) this.titleOverride.set(meta.title);
    if (meta.eyebrow !== undefined) this.eyebrowOverride.set(meta.eyebrow);
  }

  clear() {
    this.badge.set(null);
    this.description.set(null);
    this.titleOverride.set(null);
    this.eyebrowOverride.set(null);
  }
}
