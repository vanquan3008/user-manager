import { Component, computed, input, signal } from '@angular/core';

export interface StackedBarPoint {
  label: string;
  /** Full label for tooltip / table (e.g. a long date). */
  detail: string;
  values: number[];
}

export interface StackedBarSeries {
  name: string;
  /** CSS color, preferably a token such as var(--series-1). */
  color: string;
}

/**
 * Accessible stacked column chart (SVG): thin columns (≤24px) with 4px rounded tops, 2px surface
 * gaps between segments, recessive grid, per-column hover tooltip, legend and a table view toggle.
 */
@Component({
  selector: 'app-stacked-bar-chart',
  standalone: true,
  template: `
    <div class="flex flex-wrap items-center justify-between gap-3">
      <div class="flex flex-wrap items-center gap-4" role="list" aria-label="Legend">
        @for (s of series(); track s.name; let i = $index) {
          <span class="inline-flex items-center gap-1.5 text-xs text-fg-2" role="listitem">
            <span class="h-2.5 w-2.5 rounded-sm" [style.background]="s.color"></span>{{ s.name }}
            <span class="font-medium tabular-nums text-fg">{{ totals()[i] }}</span>
          </span>
        }
      </div>
      <div class="segmented">
        <button type="button" [class.is-active]="!showTable()" (click)="showTable.set(false)">{{ chartLabel() }}</button>
        <button type="button" [class.is-active]="showTable()" (click)="showTable.set(true)">{{ tableLabel() }}</button>
      </div>
    </div>

    @if (showTable()) {
      <div class="table-wrap mt-4 max-h-64 overflow-y-auto rounded-lg border border-line">
        <table class="table">
          <thead><tr><th>{{ tableLabel() }}</th>@for (s of series(); track s.name) { <th class="text-right">{{ s.name }}</th> }</tr></thead>
          <tbody>
            @for (p of points(); track p.detail) {
              <tr><td>{{ p.detail }}</td>@for (v of p.values; track $index) { <td class="text-right tabular-nums">{{ v }}</td> }</tr>
            }
          </tbody>
        </table>
      </div>
    } @else {
      <div class="relative mt-4" (mouseleave)="hover.set(null)">
        <svg [attr.viewBox]="'0 0 ' + width + ' ' + height" class="h-56 w-full overflow-visible" preserveAspectRatio="none" role="img" [attr.aria-label]="ariaLabel()">
          @for (tick of ticks(); track tick) {
            <line [attr.x1]="padLeft" [attr.x2]="width" [attr.y1]="y(tick)" [attr.y2]="y(tick)" stroke="rgb(var(--line))" stroke-width="1" vector-effect="non-scaling-stroke" [attr.stroke-dasharray]="tick === 0 ? null : '3 3'" />
          }
          @for (p of points(); track p.detail; let i = $index) {
            <g>
              <!-- generous hit target -->
              <rect [attr.x]="slotX(i)" y="0" [attr.width]="slot()" [attr.height]="height" fill="transparent" (mouseenter)="hover.set(i)" />
              @if (hover() === i) {
                <rect [attr.x]="slotX(i)" y="0" [attr.width]="slot()" [attr.height]="baseline" fill="rgb(var(--surface-2))" opacity="0.8" pointer-events="none" />
              }
              @for (seg of segments(i); track $index) {
                <path [attr.d]="seg.d" [attr.fill]="seg.color" pointer-events="none" />
              }
            </g>
          }
        </svg>
        <div class="pointer-events-none absolute inset-x-0 flex text-2xs text-fg-3" [style.padding-left.%]="padLeft / width * 100">
          @for (p of points(); track p.detail; let i = $index) {
            <span class="flex-1 text-center tabular-nums" [class.invisible]="!showTick(i)">{{ p.label }}</span>
          }
        </div>
        <div class="pointer-events-none absolute left-0 top-0 h-56 text-2xs tabular-nums text-fg-3" aria-hidden="true">
          @for (tick of ticks(); track tick) {
            <span class="absolute left-0 -translate-y-1/2 leading-none" [style.top.%]="y(tick) / height * 100">{{ tick }}</span>
          }
        </div>

        @if (hover() !== null) {
          @let p = points()[hover()!];
          <div class="pointer-events-none absolute top-2 z-10 min-w-[9rem] rounded-lg border border-line bg-surface px-3 py-2 text-xs shadow-pop"
               [style.left.%]="tooltipLeft(hover()!)" [class.-translate-x-full]="hover()! > points().length / 2">
            <p class="mb-1.5 font-medium text-fg">{{ p.detail }}</p>
            @for (s of series(); track s.name; let j = $index) {
              <p class="flex items-center justify-between gap-4 text-fg-2">
                <span class="inline-flex items-center gap-1.5"><span class="h-2 w-2 rounded-sm" [style.background]="s.color"></span>{{ s.name }}</span>
                <span class="font-medium tabular-nums text-fg">{{ p.values[j] }}</span>
              </p>
            }
          </div>
        }
      </div>
    }
  `
})
export class StackedBarChartComponent {
  points = input.required<StackedBarPoint[]>();
  series = input.required<StackedBarSeries[]>();
  ariaLabel = input<string>('Chart');
  chartLabel = input<string>('Chart');
  tableLabel = input<string>('Table');

  readonly width = 640;
  readonly height = 224;
  readonly baseline = 224;
  readonly padLeft = 28;
  readonly hover = signal<number | null>(null);
  readonly showTable = signal(false);

  readonly totals = computed(() => this.series().map((_, j) => this.points().reduce((sum, p) => sum + (p.values[j] ?? 0), 0)));
  readonly max = computed(() => {
    const m = Math.max(1, ...this.points().map(p => p.values.reduce((a, b) => a + b, 0)));
    const step = this.niceStep(m);
    return Math.ceil(m / step) * step;
  });
  readonly ticks = computed(() => {
    const step = this.niceStep(this.max());
    const out: number[] = [];
    for (let v = 0; v <= this.max(); v += step) out.push(v);
    return out;
  });
  readonly ticksDesc = computed(() => [...this.ticks()].reverse());
  readonly slot = computed(() => (this.width - this.padLeft) / Math.max(1, this.points().length));

  y(value: number) { return this.baseline - (value / this.max()) * (this.baseline - 8); }
  slotX(i: number) { return this.padLeft + i * this.slot(); }
  showTick(i: number) {
    const n = this.points().length;
    return n <= 8 || i % Math.ceil(n / 7) === 0 || i === n - 1;
  }
  tooltipLeft(i: number) { return ((this.slotX(i) + this.slot() / 2) / this.width) * 100; }

  /** Segment paths: square at the baseline, 4px radius on the top end of the stack, 2px gaps. */
  segments(i: number) {
    const p = this.points()[i];
    const barW = Math.min(24, this.slot() * 0.56);
    const x = this.slotX(i) + (this.slot() - barW) / 2;
    const out: { d: string; color: string }[] = [];
    let acc = 0;
    const lastIndex = p.values.map((v, k) => (v > 0 ? k : -1)).reduce((a, b) => Math.max(a, b), -1);
    p.values.forEach((v, k) => {
      if (v <= 0) return;
      const yBottom = this.y(acc) - (acc > 0 ? 2 : 0);
      acc += v;
      const yTop = this.y(acc);
      const h = Math.max(1, yBottom - yTop);
      const r = k === lastIndex ? Math.min(4, h, barW / 2) : 0;
      const d = r > 0
        ? `M${x},${yBottom} V${yTop + r} Q${x},${yTop} ${x + r},${yTop} H${x + barW - r} Q${x + barW},${yTop} ${x + barW},${yTop + r} V${yBottom} Z`
        : `M${x},${yBottom} V${yTop} H${x + barW} V${yBottom} Z`;
      out.push({ d, color: this.series()[k]?.color ?? 'currentColor' });
    });
    return out;
  }

  private niceStep(max: number) {
    const raw = max / 4;
    const mag = Math.pow(10, Math.floor(Math.log10(Math.max(raw, 1))));
    const norm = raw / mag;
    return (norm <= 1 ? 1 : norm <= 2 ? 2 : norm <= 5 ? 5 : 10) * mag;
  }
}
