export const environment = {
  production: false,
  apiUrl: process.env['NG_APP_API_URL'],
  defaultListPageSize: 25,
  listPageSizeOptions: [10, 25, 50, 100] as const,
  usersPageSize: 25,
  maxImageUploadBytes: 2 * 1024 * 1024,
  uploadFolderAppLogos: 'app-logos',
  defaultAppLogoDataUri:
    'data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 64 64"><rect width="64" height="64" rx="12" fill="%230ea5e9"/><path d="M20 42V22h8.5c6.5 0 10.5 3.8 10.5 10s-4 10-10.5 10H20zm6-5h2.2c3.8 0 6.2-2.1 6.2-5s-2.4-5-6.2-5H26v10zM42 42V22h6v20h-6z" fill="white"/></svg>',

  oauth: {
    clientId: process.env['NG_APP_OAUTH_CLIENT_ID'] || '',
    defaultScopes: ['profile', 'email', 'openid'],
    redirectUri: process.env['NG_APP_OAUTH_REDIRECT_URI'] || '',
    codeChallengeMethod: 'S256'
  }
};
