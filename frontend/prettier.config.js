export default {
  semi: true,
  trailingComma: 'es5',
  singleQuote: true,
  printWidth: 80,
  tabWidth: 2,
  useTabs: false,
  bracketSpacing: true,
  bracketSameLine: false,
  arrowParens: 'avoid',
  endOfLine: 'lf',
  overrides: [
    {
      files: ['*.ts', '*.tsx', '*.json', '*.css', '*.md', '*.html'],
      options: {
        tabWidth: 2,
      },
    },
  ],
} 