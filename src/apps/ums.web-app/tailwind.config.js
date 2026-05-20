/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        brand: {
          50: '#eef2ff',
          100: '#e0e7ff',
          200: '#c7d2fe',
          300: '#a5b4fc',
          400: '#818cf8',
          500: '#6366f1',
          600: '#4f46e5',
          700: '#4338ca',
          800: '#3730a3',
          900: '#312e81',
          950: '#1e1b4b',
        },
        dark: {
          50: '#f8fafc',
          100: '#f1f5f9',
          900: '#0f172a',
          950: '#020617',
        },
        m3: {
          primary: 'hsl(var(--m3-primary))',
          'on-primary': 'hsl(var(--m3-on-primary))',
          'primary-container': 'hsl(var(--m3-primary-container))',
          'on-primary-container': 'hsl(var(--m3-on-primary-container))',
          secondary: 'hsl(var(--m3-secondary))',
          'on-secondary': 'hsl(var(--m3-on-secondary))',
          surface: 'hsl(var(--m3-surface))',
          'on-surface': 'hsl(var(--m3-on-surface))',
          'surface-container': 'hsl(var(--m3-surface-container))',
          outline: 'hsl(var(--m3-outline))',
          error: 'hsl(var(--m3-error))',
          'on-error': 'hsl(var(--m3-on-error))',
        }
      },
      fontFamily: {
        sans: ['Inter', 'sans-serif'],
      },
    },
  },
  plugins: [],
}
