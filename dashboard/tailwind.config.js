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
          bg: '#14100E',
          dark: '#181310',
          card: '#1E1814',
          surface: '#261E19',
          border: '#342821',
          borderLight: '#46362D',
          hover: '#2A211B',
          muted: '#7A6C62',
          subtext: '#A8998C',
          text: '#F5EFEA',
          heading: '#FFFFFF',
          blue: '#E87722',
          blueHover: '#C96316',
          blueLight: 'rgba(232, 119, 34, 0.15)',
          amber: '#E88024',
          amberDark: '#B85B12',
          amberLight: 'rgba(232, 128, 36, 0.15)',
          green: '#2DC870',
          greenLight: 'rgba(45, 200, 112, 0.15)',
          red: '#E5484D',
          redLight: 'rgba(229, 72, 77, 0.15)',
        }
      },
      fontFamily: {
        sans: ['Plus Jakarta Sans', 'Inter', 'system-ui', 'sans-serif'],
      },
      boxShadow: {
        'card': '0 4px 20px -2px rgba(0, 0, 0, 0.6)',
        'subtle': '0 2px 8px 0 rgba(0, 0, 0, 0.4)',
        'elevated': '0 8px 30px rgba(0, 0, 0, 0.7)',
      }
    },
  },
  plugins: [],
}
