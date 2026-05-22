import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const apiUrl = env.VITE_API_URL || 'http://localhost:5293'

  return {
    plugins: [react()],
    resolve: {
      alias: {
        '@app':          path.resolve(__dirname, 'src/application'),
        '@domain':       path.resolve(__dirname, 'src/domain'),
        '@infra':        path.resolve(__dirname, 'src/infrastructure'),
        '@presentation': path.resolve(__dirname, 'src/presentation'),
        '@shared':       path.resolve(__dirname, 'src/presentation/shared'),
      },
    },
    server: {
      port: 5173,
      // CSP must be delivered via HTTP headers; 'frame-ancestors' is ignored in <meta> tags.
      // Production nginx.conf is the source of truth; these headers mirror it for dev parity.
      headers: {
        'Content-Security-Policy': "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com; img-src 'self' data: blob: https:; connect-src 'self' https: ws: wss:; frame-ancestors 'none'; base-uri 'self'; form-action 'self';",
        'X-Content-Type-Options': 'nosniff',
        'Referrer-Policy': 'strict-origin-when-cross-origin',
        'X-Frame-Options': 'DENY',
      },
      proxy: {
        '/api': {
          target: apiUrl,
          changeOrigin: true,
          secure: false,
          timeout: 10_000,
          proxyTimeout: 10_000,
        },
        '/graphql': {
          target: apiUrl,
          changeOrigin: true,
          secure: false,
          timeout: 10_000,
          proxyTimeout: 10_000,
        },
      },
    },
    build: {
      target: 'es2020',
      sourcemap: mode === 'development',
      minify: 'esbuild',
      rollupOptions: {
        output: {
          manualChunks: {
            'react-vendor': ['react', 'react-dom'],
            'tanstack-query': ['@tanstack/react-query'],
            'lucide-icons': ['lucide-react'],
          },
        },
      },
    },
    optimizeDeps: {
      include: ['react', 'react-dom', '@tanstack/react-query', 'zustand'],
    },
  }
})
