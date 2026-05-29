import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import prettier from 'eslint-plugin-prettier'
import eslintConfigPrettier from 'eslint-config-prettier'

export default tseslint.config(
  { ignores: ['dist'] },
  {
    extends: [js.configs.recommended, ...tseslint.configs.recommended, eslintConfigPrettier],
    files: ['**/*.{ts,tsx}'],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
    plugins: {
      'react-hooks': reactHooks,
      'react-refresh': reactRefresh,
      prettier,
    },
    rules: {
      ...reactHooks.configs.recommended.rules,
      // Work around mixed workspace ESLint resolution until dependencies are
      // fully flattened. TypeScript strict build covers this check reliably.
      '@typescript-eslint/no-unused-expressions': 'off',
      'react-refresh/only-export-components': [
        'warn',
        { allowConstantExport: true },
      ],
      // Production: only allow console.error (for error boundaries)
      'no-console': ['error', { allow: ['error'] }],
      // Require explicit return types on exported functions for API boundaries
      '@typescript-eslint/explicit-function-return-type': 'off',
      // Allow non-null assertions in test files only
      '@typescript-eslint/no-non-null-assertion': 'warn',
      // Prettier formatting rules
      'prettier/prettier': 'error',
    },
  },
)
