# syntax=docker/dockerfile:1
FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY . .
# NG_APP_* values are compiled into the bundle at build time.
ARG NG_APP_API_URL=http://localhost:5004
ARG NG_APP_OAUTH_CLIENT_ID=user-manager
ARG NG_APP_OAUTH_REDIRECT_URI=http://localhost:8080/oauth/callback
ENV NG_APP_API_URL=$NG_APP_API_URL \
    NG_APP_OAUTH_CLIENT_ID=$NG_APP_OAUTH_CLIENT_ID \
    NG_APP_OAUTH_REDIRECT_URI=$NG_APP_OAUTH_REDIRECT_URI
RUN npm run build

FROM nginxinc/nginx-unprivileged:1.27-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/user-admin-ui/browser /usr/share/nginx/html
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=3s CMD wget -qO- http://127.0.0.1:8080/healthz || exit 1
