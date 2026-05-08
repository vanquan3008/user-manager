import { Injectable, signal, computed } from '@angular/core';
import { environment } from '../../../environments/environment';

const STORAGE_KEY = 'um.listPageSize';

/**
 * Cỡ trang dùng chung (localStorage + environment), cho roles/permissions/applications và API users.
 */
@Injectable({ providedIn: 'root' })
export class ListPageSizeService {
  readonly options = [...environment.listPageSizeOptions] as number[];

  readonly pageSize = signal(this.readInitial());

  readonly pageSizeSelectOptions = computed(() =>
    this.options.map((n) => ({ value: n, label: String(n) }))
  );

  setPageSize(size: number): void {
    if (!this.options.includes(size)) {
      return;
    }
    try {
      localStorage.setItem(STORAGE_KEY, String(size));
    } catch {
      /* ignore */
    }
    this.pageSize.set(size);
  }

  private readInitial(): number {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      const n = raw ? parseInt(raw, 10) : NaN;
      if (Number.isFinite(n) && this.options.includes(n)) {
        return n;
      }
    } catch {
      /* ignore */
    }
    return environment.defaultListPageSize;
  }
}
