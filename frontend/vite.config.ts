import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'https://localhost:7038',  // BFF API Gateway
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path  // Mantener el path /api/*
      },
    },
  },
})
