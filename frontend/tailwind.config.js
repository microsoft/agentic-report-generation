/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        background: {
          DEFAULT: '#0F0F0F',
          surface: '#1E1E1E',
          input: '#2A2A2A',
          message: '#343541',
          user: '#343541',
        },
        text: {
          DEFAULT: '#FFFFFF',
          secondary: '#A3A3A3',
          muted: '#737373',
        },
        border: {
          DEFAULT: 'rgba(255,255,255,0.1)',
          hover: 'rgba(255,255,255,0.2)',
        },
        button: {
          DEFAULT: '#2A2A2A',
          hover: '#404040',
        }
      },
    },
  },
  plugins: [],
} 