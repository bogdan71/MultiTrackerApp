import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: parseInt(process.env.PORT || '5173'),
    proxy: {
      '/api': {
        target: process.env.services__tracker_api__https__0 
          || process.env.services__tracker_api__http__0 
          || 'https://localhost:5001',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
