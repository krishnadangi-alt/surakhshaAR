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
        suraksha: {
          bg: '#F8FAFC',
          dark: '#0F172A',
          card: '#FFFFFF',
          surface: '#F1F5F9',
          border: '#E2E8F0',
          borderLight: '#CBD5E1',
          hover: '#F8FAFC',
          muted: '#94A3B8',
          subtext: '#64748B',
          text: '#0F172A',
          heading: '#0F172A',
          blue: '#1D6BF3',
          blueHover: '#1556C7',
          blueLight: 'rgba(29, 107, 243, 0.08)',
          amber: '#D97706',
          amberDark: '#B45309',
          amberLight: 'rgba(217, 119, 6, 0.08)',
          green: '#059669',
          greenLight: 'rgba(5, 150, 105, 0.08)',
          red: '#DC2626',
          redLight: 'rgba(220, 38, 38, 0.08)',
        }
      },
      fontFamily: {
        sans: ['Plus Jakarta Sans', 'Inter', 'system-ui', 'sans-serif'],
      },
      boxShadow: {
        'card': '0 1px 3px 0 rgba(15, 23, 42, 0.08), 0 1px 2px 0 rgba(15, 23, 42, 0.04)',
        'subtle': '0 1px 2px 0 rgba(15, 23, 42, 0.05)',
        'elevated': '0 10px 25px -5px rgba(15, 23, 42, 0.12), 0 8px 10px -6px rgba(15, 23, 42, 0.08)',
      }
    },
  },
  plugins: [],
}
