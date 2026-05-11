# =========================================================
# Phase 1: Dependency Resolution
# =========================================================
FROM node:20-alpine AS base
WORKDIR /usr/src/app
COPY package*.json ./
COPY apps/web/package*.json ./apps/web/
RUN npm ci

# =========================================================
# Phase 2: Production Compilation
# =========================================================
FROM base AS builder
COPY . .
RUN npm run build --workspace=apps/web

# =========================================================
# Phase 3: Secure Static Server (Nginx)
# =========================================================
FROM nginx:1.25-alpine AS runner

# Copy Vite-compiled static assets to Nginx html directory
COPY --from=builder /usr/src/app/apps/web/dist /usr/share/nginx/html

# Custom lightweight Nginx config to support SPA fallback routing & security headers
RUN echo 'server { \
    listen 80; \
    server_name localhost; \
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
CMD ["nginx", "-g", "daemon off;"]
