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
        /**
         * Full MD3 colour role set.
         * Each token uses hsl(var(--token)) so Tailwind opacity modifiers
         * such as bg-m3-primary/10 work correctly.
         */
        m3: {
          // Primary
          primary:                  'hsl(var(--m3-primary))',
          'on-primary':             'hsl(var(--m3-on-primary))',
          'primary-container':      'hsl(var(--m3-primary-container))',
          'on-primary-container':   'hsl(var(--m3-on-primary-container))',
          // Secondary
          secondary:                'hsl(var(--m3-secondary))',
          'on-secondary':           'hsl(var(--m3-on-secondary))',
          'secondary-container':    'hsl(var(--m3-secondary-container))',
          'on-secondary-container': 'hsl(var(--m3-on-secondary-container))',
          // Tertiary
          tertiary:                 'hsl(var(--m3-tertiary))',
          'on-tertiary':            'hsl(var(--m3-on-tertiary))',
          'tertiary-container':     'hsl(var(--m3-tertiary-container))',
          'on-tertiary-container':  'hsl(var(--m3-on-tertiary-container))',
          // Error
          error:                    'hsl(var(--m3-error))',
          'on-error':               'hsl(var(--m3-on-error))',
          'error-container':        'hsl(var(--m3-error-container))',
          'on-error-container':     'hsl(var(--m3-on-error-container))',
          // Surface
          surface:                  'hsl(var(--m3-surface))',
          'on-surface':             'hsl(var(--m3-on-surface))',
          'surface-variant':        'hsl(var(--m3-surface-variant))',
          'on-surface-variant':     'hsl(var(--m3-on-surface-variant))',
          'surface-container':      'hsl(var(--m3-surface-container))',
          'surface-container-low':  'hsl(var(--m3-surface-container-low))',
          'surface-container-high': 'hsl(var(--m3-surface-container-high))',
          // Outline
          outline:                  'hsl(var(--m3-outline))',
          'outline-variant':        'hsl(var(--m3-outline-variant))',
          // Inverse
          'inverse-surface':        'hsl(var(--m3-inverse-surface))',
          'inverse-on-surface':     'hsl(var(--m3-inverse-on-surface))',
          'inverse-primary':        'hsl(var(--m3-inverse-primary))',
        }
      },
      fontFamily: {
        sans: ['Inter', 'sans-serif'],
      },
      keyframes: {
        tooltipIn: {
          '0%': { opacity: '0', transform: 'scale(0.92) translateY(3px)' },
          '100%': { opacity: '1', transform: 'scale(1) translateY(0)' },
        },
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        slideDown: {
          '0%': { opacity: '0', transform: 'translateY(-6px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
      },
      animation: {
        tooltipIn: 'tooltipIn 0.12s ease-out forwards',
        fadeIn: 'fadeIn 0.18s ease-out forwards',
        slideDown: 'slideDown 0.18s ease-out forwards',
      },
    },
  },
  plugins: [],
}
