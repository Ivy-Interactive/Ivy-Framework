/** @type {import('tailwindcss').Config} */
export default {
  content: ['./src/**/*.{js,ts,jsx,tsx}'],
  // Use the host app's theme - external widgets inherit Tailwind classes from the host
  // This config is mainly for development/preview purposes
  theme: {
    extend: {},
  },
  plugins: [],
};
