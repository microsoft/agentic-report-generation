/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: '#0a66c2', // LinkedIn’s signature blue
        secondary: '#004182', // A deeper blue
        surface: '#f3f2ef',   // LinkedIn's subtle background 
        accent: '#067dbb',   // Another accent color
        // Text colors
        textDefault: '#222222',
        textLight: '#666666',
        textMuted: '#8c8c8c',
        // For ChatInterface backgrounds, etc.
        backgroundSurface: '#ffffff',
        backgroundMessage: '#eef3f8',
        buttonHover: '#e1e9f1',
        borderDefault: '#d5d5d5',
      },
    },
  },
  plugins: [],
} 