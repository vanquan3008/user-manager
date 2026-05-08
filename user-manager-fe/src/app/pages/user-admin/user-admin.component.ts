import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ThemeService } from '../../core/services/theme.service';
import { TranslateService } from '../../core/services/translate.service';
import { PageToolbarService } from '../../core/services/page-toolbar.service';
import { NotificationService } from '../../core/services/notification.service';
import { DrawerComponent } from '../../shared/components/drawer/drawer.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';
import { DrawerFooterActionsComponent } from '../../shared/components/drawer-footer-actions/drawer-footer-actions.component';
import { IconButtonComponent } from '../../shared/components/icon-button/icon-button.component';

type AdminTab = 'users' | 'roles' | 'plans';

interface AdminUser {
  name: string;
  email: string;
  role: string;
  status: 'active' | 'pending' | 'banned';
  plan: string;
}

interface AdminRole {
  name: string;
  description: string;
  users: number;
  permissions: string[];
}

interface AdminPlan {
  name: string;
  price: string;
  period: string;
  recommended?: boolean;
  limits: string[];
}

@Component({
  selector: 'app-user-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, DrawerComponent, FormFieldComponent, DrawerFooterActionsComponent, IconButtonComponent],
  templateUrl: './user-admin.component.html'
})
export class UserAdminComponent implements OnInit {
  theme = inject(ThemeService).theme;
  translate = inject(TranslateService);
  private pageToolbar = inject(PageToolbarService);
  private notify = inject(NotificationService);
  activeTab: AdminTab = 'users';

  ngOnInit() {
    this.pageToolbar.setMeta({
      badge: 'CRM',
      description: this.translate.t().userAdminDesc
    });
  }

  search = '';
  showCreateModal = signal(false);
  closingCreate = signal(false);
  savingCreate = signal(false);
  createForm = {
    name: '',
    email: '',
    role: '',
    plan: '',
    status: 'active' as 'active' | 'pending' | 'banned'
  };

  get activeCount(): number {
    return this.users.filter(u => u.status === 'active').length;
  }

  get bannedCount(): number {
    return this.users.filter(u => u.status === 'banned').length;
  }

  users: AdminUser[] = [
    {
      name: 'Mai Khanh',
      email: 'mai.khanh@example.com',
      role: 'Owner',
      status: 'active',
      plan: 'Pro'
    },
    {
      name: 'Nguyễn Nam',
      email: 'nam.nguyen@example.com',
      role: 'Marketing Lead',
      status: 'active',
      plan: 'Growth'
    },
    {
      name: 'Trần Minh Anh',
      email: 'minh.anh@example.com',
      role: 'Analyst',
      status: 'pending',
      plan: 'Free'
    },
    {
      name: 'CozyHome Team',
      email: 'team@cozyhome.vn',
      role: 'Agency',
      status: 'banned',
      plan: 'Enterprise'
    }
  ];

  roles: AdminRole[] = [
    {
      name: 'Owner',
      description: 'Toàn quyền quản trị hệ thống và thanh toán.',
      users: 1,
      permissions: ['Quản lý user', 'Phân quyền', 'Quản lý gói', 'Thiết lập thanh toán']
    },
    {
      name: 'Marketing Lead',
      description: 'Quản lý chiến dịch, phân tích hiệu suất và team.',
      users: 3,
      permissions: ['Quản lý user', 'Quản lý chiến dịch', 'Xem báo cáo']
    },
    {
      name: 'Analyst',
      description: 'Truy cập số liệu và xuất báo cáo.',
      users: 4,
      permissions: ['Xem báo cáo', 'Xuất dữ liệu']
    }
  ];

  plans: AdminPlan[] = [
    {
      name: 'Free',
      price: '0đ',
      period: '/tháng',
      limits: ['Tối đa 1 workspace', 'Tối đa 3 thành viên', 'Giới hạn 2 kênh social']
    },
    {
      name: 'Pro',
      price: '490.000đ',
      period: '/tháng',
      recommended: true,
      limits: [
        'Tối đa 3 workspace',
        'Tối đa 10 thành viên',
        'Không giới hạn kênh social',
        'Báo cáo nâng cao'
      ]
    },
    {
      name: 'Enterprise',
      price: 'Liên hệ',
      period: '',
      limits: [
        'Không giới hạn workspace',
        'Không giới hạn thành viên',
        'Hỗ trợ SLA riêng',
        'SSO / SCIM / Audit log'
      ]
    }
  ];

  setTab(tab: AdminTab) {
    this.activeTab = tab;
  }

  openCreateUser() {
    this.createForm = {
      name: '',
      email: '',
      role: this.roles[0]?.name ?? '',
      plan: this.plans[0]?.name ?? 'Free',
      status: 'active'
    };
    this.showCreateModal.set(true);
    this.closingCreate.set(false);
  }

  closeCreateModal() {
    this.closingCreate.set(true);
    setTimeout(() => {
      this.showCreateModal.set(false);
      this.closingCreate.set(false);
    }, 250);
  }

  saveCreateUser() {
    const f = this.createForm;
    if (!f.name.trim()) {
      this.notify.error('Tên không được để trống.');
      return;
    }
    if (!f.email.trim()) {
      this.notify.error('Email không được để trống.');
      return;
    }
    if (!f.role) {
      this.notify.error('Vui lòng chọn role.');
      return;
    }
    this.savingCreate.set(true);
    const newUser: AdminUser = {
      name: f.name.trim(),
      email: f.email.trim(),
      role: f.role || (this.roles[0]?.name ?? 'Analyst'),
      status: f.status,
      plan: f.plan || (this.plans[0]?.name ?? 'Free')
    };
    this.users = [...this.users, newUser];
    this.closeCreateModal();
    this.notify.success('Đã thêm user.');
    this.savingCreate.set(false);
  }

  filteredUsers(): AdminUser[] {
    const term = this.search.toLowerCase().trim();
    if (!term) {
      return this.users;
    }
    return this.users.filter(
      u =>
        u.name.toLowerCase().includes(term) ||
        u.email.toLowerCase().includes(term) ||
        u.role.toLowerCase().includes(term) ||
        u.plan.toLowerCase().includes(term)
    );
  }
}

