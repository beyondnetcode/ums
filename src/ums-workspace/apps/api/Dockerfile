# =========================================================
# Phase 1: Dependency Resolution & Shared Cache
# =========================================================
FROM node:20-alpine AS base
WORKDIR /usr/src/app
COPY package*.json ./
COPY apps/api/package*.json ./apps/api/
COPY libs/aop/package*.json ./libs/aop/
RUN npm ci

# =========================================================
# Phase 2: Application Build & Compilation
# =========================================================
FROM base AS builder
COPY . .
RUN npm run build --workspace=apps/api

# =========================================================
# Phase 3: Pruned Production Node Modules
# =========================================================
FROM node:20-alpine AS production-deps
WORKDIR /usr/src/app
COPY package*.json ./
COPY apps/api/package*.json ./apps/api/
COPY libs/aop/package*.json ./libs/aop/
RUN npm ci --only=production

# =========================================================
# Phase 4: Secure Production Runtime
# =========================================================
FROM node:20-alpine AS runner
WORKDIR /usr/src/app
ENV NODE_ENV=production

# Create non-privileged user for security hardening
RUN addgroup -g 1001 -S nodejs && adduser -S nestjs -u 1001

# Copy compiled artifacts and production dependencies
COPY --from=production-deps /usr/src/app/node_modules ./node_modules
COPY --from=builder /usr/src/app/apps/api/dist ./dist
COPY --from=builder /usr/src/app/apps/api/package.json ./package.json

# Inject shared AOP libraries if present
COPY --from=builder /usr/src/app/libs/aop ./libs/aop

USER nestjs

EXPOSE 3000
ENV PORT=3000

CMD ["node", "dist/main.js"]
