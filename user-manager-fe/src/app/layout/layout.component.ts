import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { NavigationEnd, NavigationStart, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../core/services/auth.service';
import { ThemeService } from '../core/services/theme.service';
import { TranslateService, type Locale } from '../core/services/translate.service';
import { NotificationComponent } from '../shared/components/notification/notification.component';
import { ConfirmDialogComponent } from '../shared/components/confirm-dialog/confirm-dialog.component';
import { PageToolbarService } from '../core/services/page-toolbar.service';

type NavItem = {
  path: string;
  icon: string;
  label: string;
  exact?: boolean;
  group: 'primary' | 'secondary';
};

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet, NotificationComponent, ConfirmDialogComponent],
  templateUrl: './layout.component.html'
})
export class LayoutComponent {
  readonly auth = inject(AuthService);
  readonly themeService = inject(ThemeService);
  readonly translate = inject(TranslateService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  readonly pageToolbar = inject(PageToolbarService);
  private lastNavPath = '';

  readonly theme = this.themeService.theme;
  readonly sidebarOpen = signal(false);
  readonly pageTitle = signal(this.translate.t().layoutPageDashboard);
  readonly pageEyebrow = signal(this.translate.t().layoutEyebrowOverview);
  readonly headerTitle = computed(() => this.pageToolbar.titleOverride() ?? this.pageTitle());
  readonly headerEyebrow = computed(() => this.pageToolbar.eyebrowOverride() ?? this.pageEyebrow());
  readonly localeOptions: Locale[] = ['vi', 'en', 'zh'];
  readonly navItems = computed<NavItem[]>(() => {
    const t = this.translate.t();
    return [
      { path: '/', icon: 'space_dashboard', label: t.navOverview, exact: true, group: 'primary' },
      { path: '/users', icon: 'group', label: t.navUsers, group: 'primary' },
      { path: '/applications', icon: 'apps', label: t.navApplications, group: 'primary' },
      { path: '/roles', icon: 'shield_person', label: t.navRoles, group: 'primary' },
      { path: '/subscriptions', icon: 'workspace_premium', label: t.navSubscriptions, group: 'primary' },
      { path: '/package-catalog', icon: 'inventory_2', label: t.navCatalog, group: 'secondary' },
      { path: '/permissions', icon: 'key', label: t.navPermissions, group: 'secondary' },
      { path: '/role-permission', icon: 'link', label: t.navRolePermission, group: 'secondary' },
      { path: '/change-password', icon: 'lock', label: t.navSecurity, group: 'secondary' }
    ];
  });
  readonly primaryNav = computed(() => this.navItems().filter(item => item.group === 'primary'));
  readonly secondaryNav = computed(() => this.navItems().filter(item => item.group === 'secondary'));

  setLocale(locale: Locale) {
    this.translate.setLocale(locale);
    this.updateTitle();
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }

  toggleSidebar() {
    this.sidebarOpen.update(value => !value);
  }

  closeSidebar() {
    this.sidebarOpen.set(false);
  }

  constructor() {
    this.updateTitle();
    this.router.events
      .pipe(
        filter((event): event is NavigationStart => event instanceof NavigationStart),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((event: NavigationStart) => {
        const nextPath = event.url.split('?')[0];
        if (nextPath !== this.lastNavPath) {
          this.pageToolbar.clear();
        }
        this.lastNavPath = nextPath;
      });

    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.closeSidebar();
        this.updateTitle();
      });
  }

  private updateTitle() {
    const t = this.translate.t();
    const [segment1 = '', segment2 = ''] = this.router.url.split('?')[0].split('/').filter(Boolean);

    if (!segment1) {
      this.pageTitle.set(t.layoutPageDashboard);
      this.pageEyebrow.set(t.layoutEyebrowOverview);
      return;
    }

    const map: Record<string, { title: string; eyebrow: string }> = {
      users: {
        title: t.layoutPageUsers,
        eyebrow: segment2 ? t.layoutEyebrowUserDetail : t.layoutEyebrowIdentity
      },
      applications: {
        title: t.layoutPageApplications,
        eyebrow: segment2 ? t.layoutEyebrowAppDetail : t.layoutEyebrowAppList
      },
      roles: { title: t.layoutPageRoles, eyebrow: t.layoutEyebrowAuthModel },
      subscriptions: { title: t.layoutPageSubscriptions, eyebrow: t.layoutEyebrowCommerce },
      'package-catalog': { title: t.layoutPagePackageCatalog, eyebrow: t.layoutEyebrowPackages },
      permissions: { title: t.layoutPagePermissions, eyebrow: t.layoutEyebrowAccessControl },
      'role-permission': { title: t.layoutPageRoleMapping, eyebrow: t.layoutEyebrowPermAssignment },
      'change-password': {
        title: t.layoutPageSecuritySettings,
        eyebrow: t.layoutEyebrowAccountSettings
      },
      'user-admin': { title: t.layoutPageUserAdmin, eyebrow: t.layoutEyebrowCrmWorkspace }
    };

    const current = map[segment1] ?? { title: t.layoutPageAdmin, eyebrow: t.layoutEyebrowWorkspace };
    this.pageTitle.set(current.title);
    this.pageEyebrow.set(current.eyebrow);
  }
}
