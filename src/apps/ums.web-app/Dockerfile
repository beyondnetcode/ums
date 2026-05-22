# =========================================================
# Phase 1: Dependency Resolution
# =========================================================
FROM node:20-alpine AS base
WORKDIR /usr/src/app
COPY package*.json ./
COPY apps/ums.web-app/package*.json ./apps/ums.web-app/
RUN npm ci

# =========================================================
# Phase 2: Production Compilation
# =========================================================
FROM base AS builder
COPY . .
RUN npm run build --workspace=apps/ums.web-app

# =========================================================
# Phase 3: Secure Static Server (Nginx)
# =========================================================
FROM nginx:1.25-alpine AS runner

# Copy Vite-compiled static assets to Nginx html directory
COPY --from=builder /usr/src/app/apps/ums.web-app/dist /usr/share/nginx/html

# Custom lightweight Nginx config to support SPA fallback routing & security headers
RUN echo 'server { \
    listen 80; \
    server_name localhost; \
    \
    # Security headers \
    add_header X-Frame-Options "DENY" always; \
    add_header X-Content-Type-Options "nosniff" always; \
    add_header X-XSS-Protection "1; mode=block" always; \
    add_header Referrer-Policy "strict-origin-when-cross-origin" always; \
    add_header Content-Security-Policy "default-src '"'"'self'"'"'; script-src '"'"'self'"'"' '"'"'unsafe-inline'"'"'; style-src '"'"'self'"'"' '"'"'unsafe-inline'"'"' https://fonts.googleapis.com; font-src '"'"'self'"'"' https://fonts.gstatic.com; img-src '"'"'self'"'"' data: blob: https:; connect-src '"'"'self'"'"' https:; frame-ancestors '"'"'none'"'"'; base-uri '"'"'self'"'"'; form-action '"'"'self'"'"';" always; \
    add_header Permissions-Policy "camera=(), microphone=(), geolocation=()" always; \
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always; \
    \
    location / { \
        root /usr/share/nginx/html; \
        index index.html index.htm; \
        try_files $uri $uri/ /index.html; \
    } \
    error_page 500 502 503 504 /50x.html; \
    location = /50x.html { \
        root /usr/share/nginx/html; \
    } \
}' > /etc/nginx/conf.d/default.conf

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:80/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
