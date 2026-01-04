/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        amber: {
          600: '#d97706',
          700: '#b45309',
        },
      },
    },
  },
  plugins: [],
}
