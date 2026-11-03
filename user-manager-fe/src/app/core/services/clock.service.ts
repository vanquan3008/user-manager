import { DestroyRef, Injectable, inject, signal } from '@angular/core';

/**
 * Shared wall clock for relative timestamps ("5 minutes ago").
 * Templates read one stable `now` per render pass instead of calling Date.now() while rendering,
 * and the signal ticks so the labels stay current.
 */
@Injectable({ providedIn: 'root' })
export class ClockService {
  private static readonly TickMs = 30_000;
  readonly now = signal(Date.now());

  constructor() {
    const timer = setInterval(() => this.now.set(Date.now()), ClockService.TickMs);
    inject(DestroyRef).onDestroy(() => clearInterval(timer));
  }
}
