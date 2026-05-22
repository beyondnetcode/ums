import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const apiUrl = env.VITE_API_URL || 'https://localhost:7114'

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
      proxy: {
        '/api': {
          target: apiUrl,
          changeOrigin: true,
          secure: false,
        },
        '/graphql': {
          target: apiUrl,
          changeOrigin: true,
          secure: false,
        },
      },
    },
    build: {
      target: 'es2020',
      sourcemap: mode === 'development',
      minify: 'terser',
      terserOptions: {
        compress: {
          drop_console: mode === 'production',
          drop_debugger: mode === 'production',
        },
      },
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
