import { Component, DestroyRef, ElementRef, HostListener, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, NavigationStart, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../core/services/auth.service';
import { ThemeService } from '../core/services/theme.service';
import { TranslateService, type Locale } from '../core/services/translate.service';
import { NotificationComponent } from '../shared/components/notification/notification.component';
import { ConfirmDialogComponent } from '../shared/components/confirm-dialog/confirm-dialog.component';
import { PageToolbarService } from '../core/services/page-toolbar.service';

type NavItem = { path: string; icon: string; label: string; exact?: boolean };
type NavGroup = { label: string; items: NavItem[] };

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet, NotificationComponent, ConfirmDialogComponent],
  templateUrl: './layout.component.html'
})
export class LayoutComponent {
  readonly auth = inject(AuthService);
  readonly themeService = inject(ThemeService);
  readonly translate = inject(TranslateService);
  readonly pageToolbar = inject(PageToolbarService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly host = inject(ElementRef<HTMLElement>);
  private lastNavPath = '';

  readonly theme = this.themeService.theme;
  readonly sidebarOpen = signal(false);
  readonly userMenuOpen = signal(false);
  readonly localeMenuOpen = signal(false);
  readonly currentPath = signal(this.router.url.split('?')[0]);
  readonly localeOptions: { code: Locale; label: string }[] = [
    { code: 'en', label: 'English' },
    { code: 'vi', label: 'Tiếng Việt' },
    { code: 'zh', label: '中文' }
  ];

  readonly navGroups = computed<NavGroup[]>(() => {
    const t = this.translate.t();
    return [
      { label: t.navGroupOverview, items: [{ path: '/', icon: 'space_dashboard', label: t.navDashboard, exact: true }] },
      {
        label: t.navGroupIdentity,
        items: [
          { path: '/users', icon: 'group', label: t.navUsers },
          { path: '/user-admin', icon: 'badge', label: t.navSsoAccounts }
        ]
      },
      {
        label: t.navGroupAccess,
        items: [
          { path: '/applications', icon: 'apps', label: t.navApplications },
          { path: '/roles', icon: 'shield_person', label: t.navRoles },
          { path: '/permissions', icon: 'key', label: t.navPermissions },
          { path: '/role-permission', icon: 'account_tree', label: t.navRolePermission }
        ]
      },
      {
        label: t.navGroupSecurity,
        items: [
          { path: '/audit-logs', icon: 'receipt_long', label: t.navAuditLogs },
          { path: '/system', icon: 'encrypted', label: t.navSystem }
        ]
      },
      {
        label: t.navGroupBilling,
        items: [
          { path: '/subscriptions', icon: 'workspace_premium', label: t.navSubscriptions },
          { path: '/package-catalog', icon: 'inventory_2', label: t.navCatalog }
        ]
      }
    ];
  });

  /** Breadcrumb: "Group / Page" derived from the navigation tree (detail pages may override the title). */
  readonly breadcrumb = computed(() => {
    const path = this.currentPath();
    const t = this.translate.t();
    if (path.startsWith('/account')) return { group: t.navAccount, title: this.pageToolbar.titleOverride() ?? t.navAccount };
    for (const group of this.navGroups()) {
      for (const item of group.items) {
        const match = item.exact ? path === item.path : path === item.path || path.startsWith(item.path + '/');
        if (match) return { group: group.label, title: this.pageToolbar.titleOverride() ?? item.label };
      }
    }
    return { group: t.productName, title: this.pageToolbar.titleOverride() ?? t.navDashboard };
  });

  readonly initials = computed(() => {
    const user = this.auth.user();
    const source = user?.displayName || user?.username || user?.email || '?';
    return source.split(/[\s@._-]+/).filter(Boolean).slice(0, 2).map(p => p[0]).join('').toUpperCase();
  });

  constructor() {
    this.router.events
      .pipe(filter((e): e is NavigationStart => e instanceof NavigationStart), takeUntilDestroyed(this.destroyRef))
      .subscribe(e => {
        const next = e.url.split('?')[0];
        if (next !== this.lastNavPath) this.pageToolbar.clear();
        this.lastNavPath = next;
      });

    this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd), takeUntilDestroyed(this.destroyRef))
      .subscribe(e => {
        this.currentPath.set(e.urlAfterRedirects.split('?')[0]);
        this.sidebarOpen.set(false);
        this.userMenuOpen.set(false);
      });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('[data-menu]')) {
      this.userMenuOpen.set(false);
      this.localeMenuOpen.set(false);
    }
  }

  @HostListener('document:keydown.escape')
  onEscape() {
    this.sidebarOpen.set(false);
    this.userMenuOpen.set(false);
    this.localeMenuOpen.set(false);
  }

  setLocale(locale: Locale) {
    this.translate.setLocale(locale);
    this.localeMenuOpen.set(false);
  }

  toggleTheme() { this.themeService.toggleTheme(); }
  toggleSidebar() { this.sidebarOpen.update(v => !v); }
  closeSidebar() { this.sidebarOpen.set(false); }

  signOut(everywhere = false) {
    this.userMenuOpen.set(false);
    this.auth.logout({ allSessions: everywhere });
  }

  focusMain(event: Event) {
    event.preventDefault();
    (this.host.nativeElement.querySelector('#main') as HTMLElement | null)?.focus();
  }
}
