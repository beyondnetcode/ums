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

# Copy Nginx configuration
COPY apps/ums.web-app/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:80/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
