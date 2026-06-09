<div align="center">
  <img src="https://raw.githubusercontent.com/angular/angular/main/aio/src/assets/images/logos/angular/angular.svg" alt="Angular Logo" width="120" />

  <h1>🎨 Celo User Manager Frontend</h1>
  <p>
    <b>The Modern Administrative Console for Identity & Access Management</b>
  </p>
  <p>
    <i>Inspired by Casdoor, built with <b>Angular 19</b> & <b>Tailwind CSS</b></i>
  </p>

  <!-- Badges -->
  <p>
    <img src="https://img.shields.io/badge/Angular-19.2-DD0031?logo=angular&logoColor=white" alt="Angular 19" />
    <img src="https://img.shields.io/badge/Styling-Tailwind%20CSS-38B2AC?logo=tailwind-css&logoColor=white" alt="Tailwind CSS" />
    <img src="https://img.shields.io/badge/Node-18%2B-339933?logo=node.js&logoColor=white" alt="Node.js" />
    <img src="https://img.shields.io/badge/TypeScript-5.7-3178C6?logo=typescript&logoColor=white" alt="TypeScript" />
  </p>
</div>

---

## 🌟 Overview

The **User Manager Frontend** (User Admin UI) is the companion web interface for the `user-manager-be` service. Designed as a highly responsive Single Page Application (SPA), it provides administrators with a centralized dashboard to manage users, applications, and complex role-based access control (RBAC).

This project takes architectural and UX inspiration from modern IAM solutions like **Casdoor**, delivering a clean, fast, and intuitive user experience.

---

## ✨ Core Features

### 🔐 Authentication Integration
* **Secure Login**: Seamless integration with the backend via JWT authentication.
* **Session Management**: Secure storage and handling of authentication tokens.

### 👥 User & Identity Management
* **Advanced User Directory**: Browse users with robust pagination and search (by email/username).
* **Profile Insights**: View detailed user information and account status.
* **Role Assignment**: Dynamically assign or revoke global roles (e.g., Admin, SuperAdmin, User) directly from the interface.

### 🧩 Multi-Tenant Application Control
* **Application Directory**: View all registered third-party applications integrated with the SSO system.
* **App-Specific Access Control**: Manage which roles are authorized to access specific applications, enabling strict multi-tenant data boundaries.

---

## 🛠️ Tech Stack

* **Framework:** Angular 19 (Standalone Components heavily encouraged)
* **Language:** TypeScript 5.7
* **Styling:** Tailwind CSS (Utility-first framework for rapid UI development)
* **State & Reactivity:** RxJS
* **Build Tool:** Angular CLI / Webpack

---

## 🚀 Getting Started

### Prerequisites
* **Node.js** (v18.0.0 or higher)
* **npm** (v9.0.0 or higher)
* The **[User Manager Backend (UserService.Api)](../user-manager-be)** must be running locally (default expected at `http://localhost:5000`).

### Installation & Setup

1. **Clone the repository and navigate to the frontend directory:**
   ```bash
   git clone <repository_url>
   cd user-manager-fe
   ```

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Environment Configuration:**
   The frontend reads runtime settings from `user-manager-fe/.env`.
   Required values:
   - `NG_APP_API_URL`
   - `NG_APP_OAUTH_CLIENT_ID`
   - `NG_APP_OAUTH_REDIRECT_URI`

   Do not put `client_secret` in the frontend. This app uses the OAuth authorization code flow with PKCE, so the SPA is a public client and must not store a secret in `.env`, source code, or browser-delivered config.

4. **Start the Development Server:**
   ```bash
   npm start
   ```
   The application will be available at `http://localhost:4200/`. The app will automatically reload if you change any of the source files.

---

## 🏗️ Building for Production

To generate a production-ready build:
```bash
npm run build
```
The build artifacts will be stored in the `dist/` directory, optimized and minified for deployment.

---

<p align="center">
  <i>Designed for seamless Identity and Access Management administration.</i>
</p>
