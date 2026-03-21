import js from '@eslint/js';
import globals from 'globals';
import reactHooks from 'eslint-plugin-react-hooks';
import reactRefresh from 'eslint-plugin-react-refresh';
import prettier from 'eslint-plugin-prettier';
import tseslint from 'typescript-eslint';

export default tseslint.config(
  { ignores: ['dist'] },
  {
    extends: [js.configs.recommended, ...tseslint.configs.recommended],
    files: ['**/*.{ts,tsx}'],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
    plugins: {
      'react-hooks': reactHooks,
      'react-refresh': reactRefresh,
      prettier: prettier,
    },
    rules: {
      ...reactHooks.configs.recommended.rules,
      'react-refresh/only-export-components': [
        'warn',
        { allowConstantExport: true },
      ],
      'prettier/prettier': 'error',
      'no-restricted-properties': [
        'error',
        {
          object: 'console',
          property: 'log',
          message: 'console log is not allowed. Use logger.info instead.',
        },
      ],
      // Ban wildcard imports except for React and specific allowed libraries
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            {
              group: [
                '@/widgets*',
                '@/components*',
                '@/lib*',
                '@/hooks*',
                '@/services*',
              ],
              importNames: ['*'],
              message:
                'Wildcard imports from internal modules are not allowed. Use named imports instead.',
            },
          ],
        },
      ],
      // TypeScript ESLint rule for import/export patterns
      '@typescript-eslint/no-import-type-side-effects': 'error',
    },
  }
);
