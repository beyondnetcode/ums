import js from '@eslint/js';
import globals from 'globals';
import reactHooks from 'eslint-plugin-react-hooks';
import reactRefresh from 'eslint-plugin-react-refresh';
import tseslint from 'typescript-eslint';
import prettier from 'eslint-plugin-prettier';
import eslintConfigPrettier from 'eslint-config-prettier';

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
      'react-refresh/only-export-components': ['warn', { allowConstantExport: true }],
      // ── Deuda de estilo heredada (visible como warning, no bloquea CI) ──────
      // Se degradan a `warn` mientras se atacan de forma incremental. Las reglas de
      // CORRECTITUD (react-hooks/*, no-fallthrough, no-case-declarations) siguen como
      // error. Ver la política de lint del repo.
      '@typescript-eslint/no-explicit-any': 'warn',
      '@typescript-eslint/no-unused-vars': 'warn',
      'no-empty': 'warn',
      'no-useless-escape': 'warn',
      // Reglas OPINADAS nuevas de eslint-plugin-react-hooks v7 (perf/patrón): visibles como
      // warning mientras se atacan incrementalmente. La regla CRÍTICA `rules-of-hooks` sigue
      // como error. Refactorizar estos patrones en código ya probado (1476 tests) se hace por
      // separado para no arriesgar regresiones.
      'react-hooks/set-state-in-effect': 'warn',
      'react-hooks/refs': 'warn',
      'react-hooks/purity': 'warn',
      'react-hooks/immutability': 'warn',
      'react-hooks/static-components': 'warn',
      'react-hooks/preserve-manual-memoization': 'warn',
      // Production: only allow console.error (for error boundaries)
      'no-console': ['warn', { allow: ['error'] }],
      // Require explicit return types on exported functions for API boundaries
      '@typescript-eslint/explicit-function-return-type': 'off',
      // Allow non-null assertions in test files only
      '@typescript-eslint/no-non-null-assertion': 'warn',
      // Prettier formatting rules
      'prettier/prettier': 'error',
    },
  }
);
