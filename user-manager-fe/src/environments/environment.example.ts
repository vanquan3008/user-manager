export const environment = {
  production: false,
  apiUrl: '${API_URL}',
  defaultListPageSize: 25,
  listPageSizeOptions: [10, 25, 50, 100] as const,
  usersPageSize: 25,
  maxImageUploadBytes: 2 * 1024 * 1024,
  uploadFolderAppLogos: 'app-logos',
  defaultAppLogoDataUri: '',

  oauth: {
    clientId: '${OAUTH_CLIENT_ID}',
    defaultScopes: ['profile', 'email', 'openid'],
    redirectUri: '',
    codeChallengeMethod: 'S256'
  }
};

