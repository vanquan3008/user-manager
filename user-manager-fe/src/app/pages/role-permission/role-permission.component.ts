import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { apiErrorMessage } from '../../core/utils/http-error.utils';
import { TranslateService } from '../../core/services/translate.service';
import { PageToolbarService } from '../../core/services/page-toolbar.service';
import { NotificationService } from '../../core/services/notification.service';
import { RolesService } from '../../core/services/roles.service';
import { PermissionsService } from '../../core/services/permissions.service';
import type { PermissionDto, RoleDto } from '../../core/models';

@Component({
  selector: 'app-role-permission',
  standalone: true,
  imports: [CommonModule, FormsModule, LoadingStateComponent, EmptyStateComponent],
  templateUrl: './role-permission.component.html'
})
export class RolePermissionComponent implements OnInit {
  translate = inject(TranslateService);
  readonly t = this.translate.t;

  readonly groupedRolePermissions = computed(() => {
    const groups = new Map<string, PermissionDto[]>();
    for (const p of [...this.rolePermissions()].sort((a, b) => a.code.localeCompare(b.code))) {
      const key = p.category || p.code.split('.')[0] || '—';
      groups.set(key, [...(groups.get(key) ?? []), p]);
    }
    return [...groups.entries()].map(([category, items]) => ({ category, items }));
  });

  pickPermission(id: string) {
    this.selectedPermissionToAssign.set(this.permissions().find(p => p.id === id) ?? null);
  }
  private pageToolbar = inject(PageToolbarService);
  private notify = inject(NotificationService);
  private rolesService = inject(RolesService);
  private permissionsService = inject(PermissionsService);

  activeTab = signal<'roles' | 'permissions' | 'assign-permission'>('roles');
  searchRole = signal('');
  searchPermission = signal('');

  // API data
  roles = signal<RoleDto[]>([]);
  permissions = signal<PermissionDto[]>([]);
  rolePermissions = signal<PermissionDto[]>([]);
  loadingRoles = signal(true);
  loadingPermissions = signal(true);
  loadingRolePermissions = signal(false);
  error = signal('');

  selectedRoleForPermission = signal<RoleDto | null>(null);
  selectedPermissionToAssign = signal<PermissionDto | null>(null);
  savingAssign = signal(false);
  savingRemove = signal<string | null>(null);

  filteredRoles = computed(() => {
    const list = this.roles();
    const q = this.searchRole().toLowerCase().trim();
    if (!q) return list;
    return list.filter(
      r =>
        (r.name || '').toLowerCase().includes(q) ||
        (r.description || '').toLowerCase().includes(q) ||
        (r.permissions || '').toLowerCase().includes(q)
    );
  });

  filteredPermissions = computed(() => {
    const list = this.permissions();
    const q = this.searchPermission().toLowerCase().trim();
    if (!q) return list;
    return list.filter(
      p =>
        (p.code || '').toLowerCase().includes(q) ||
        (p.description || '').toLowerCase().includes(q) ||
        (p.category || '').toLowerCase().includes(q)
    );
  });

  permissionsNotInRole = computed(() => {
    const assigned = this.rolePermissions();
    const assignedIds = new Set(assigned.map(p => p.id));
    return this.permissions().filter(p => !assignedIds.has(p.id));
  });

  ngOnInit() {
    this.pageToolbar.setMeta({
      badge: 'Role mapping',
      description: this.translate.t().rolePermDesc
    });
    this.loadRoles();
    this.loadPermissions();
  }

  loadRoles() {
    this.loadingRoles.set(true);
    this.rolesService.getRoles().subscribe({
      next: (list) => {
        this.roles.set(list);
        this.loadingRoles.set(false);
      },
      error: (err: any) => {
        this.error.set(apiErrorMessage(err));
        this.loadingRoles.set(false);
      }
    });
  }

  loadPermissions() {
    this.loadingPermissions.set(true);
    this.permissionsService.getPermissions().subscribe({
      next: (list) => {
        this.permissions.set(list);
        this.loadingPermissions.set(false);
      },
      error: (err: any) => {
        this.error.set(apiErrorMessage(err));
        this.loadingPermissions.set(false);
      }
    });
  }

  setTab(tab: 'roles' | 'permissions' | 'assign-permission') {
    this.activeTab.set(tab);
  }

  selectRoleForPermission(role: RoleDto) {
    this.selectedRoleForPermission.set(role);
    this.selectedPermissionToAssign.set(null);
    this.loadRolePermissions(role);
  }

  /** Parse role.permissions string (comma-separated or JSON) thành mảng codes */
  private parseRolePermissionsString(permissionsStr: string | undefined): string[] {
    if (!permissionsStr?.trim()) return [];
    const s = permissionsStr.trim();
    if (s.startsWith('[')) {
      try {
        const arr = JSON.parse(s) as unknown[];
        return arr.map(x => String(x ?? '')).filter(Boolean);
      } catch {
        return s.split(/[,;]/).map(x => x.trim()).filter(Boolean);
      }
    }
    return s.split(/[,;]/).map(x => x.trim()).filter(Boolean);
  }

  loadRolePermissions(role: RoleDto) {
    this.loadingRolePermissions.set(true);
    this.rolePermissions.set([]);
    const allPerms = this.permissions();

    this.permissionsService.getRolePermissions(role.id).subscribe({
      next: (list) => {
        if (list.length > 0) {
          this.rolePermissions.set(list);
        } else {
          const codesFromRole = this.parseRolePermissionsString(role.permissions);
          const matched = allPerms.filter(p => codesFromRole.includes(p.code));
          this.rolePermissions.set(matched);
        }
        this.loadingRolePermissions.set(false);
      },
      error: (err: any) => {
        const codesFromRole = this.parseRolePermissionsString(role.permissions);
        const matched = allPerms.filter(p => codesFromRole.includes(p.code));
        this.rolePermissions.set(matched);
        this.loadingRolePermissions.set(false);
      }
    });
  }

  assignPermissionToRole() {
    const role = this.selectedRoleForPermission();
    const perm = this.selectedPermissionToAssign();
    if (!role || !perm) {
      this.notify.error(this.t().rpPickPermission);
      return;
    }
    this.savingAssign.set(true);
    this.permissionsService.addPermissionToRole(role.id, perm.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success(this.t().rpAssigned);
          this.loadRolePermissions(role);
          this.selectedPermissionToAssign.set(null);
        } else {
          this.notify.error(res.message || this.t().authFailed);
        }
        this.savingAssign.set(false);
      },
      error: (err: any) => {
        this.notify.error(apiErrorMessage(err));
        this.savingAssign.set(false);
      }
    });
  }

  removePermissionFromRole(role: RoleDto, permissionId: string) {
    this.savingRemove.set(permissionId);
    this.permissionsService.removePermissionFromRole(role.id, permissionId).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success(this.t().rpRemoved);
          this.loadRolePermissions(role);
        } else {
          this.notify.error(res.message || this.t().authFailed);
        }
        this.savingRemove.set(null);
      },
      error: (err: any) => {
        this.notify.error(apiErrorMessage(err));
        this.savingRemove.set(null);
      }
    });
  }
}
