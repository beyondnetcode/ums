module.exports = {
  parser: '@typescript-eslint/parser',
  parserOptions: {
    project: 'tsconfig.json',
    tsconfigRootDir: __dirname,
    sourceType: 'module',
  },
  plugins: [
    '@typescript-eslint/eslint-plugin',
    'boundaries',
    'sonarjs'
  ],
  extends: [
    'plugin:@typescript-eslint/recommended',
    'plugin:@typescript-eslint/recommended-requiring-type-checking',
    'plugin:sonarjs/recommended'
  ],
  root: true,
  env: {
    node: true,
    jest: true,
  },
  ignorePatterns: ['.eslintrc.js', 'dist', 'node_modules', 'vite.config.ts', 'eslint.config.js'],
  settings: {
    'import/resolver': {
      typescript: {},
    },
    'boundaries/elements': [
      {
        type: 'core',
        pattern: 'apps/api/src/core/**'
      },
      {
        type: 'application',
        pattern: 'apps/api/src/application/**'
      },
      {
        type: 'infrastructure',
        pattern: 'apps/api/src/infrastructure/**'
      }
    ]
  },
  rules: {
    // 0. REGLA DE ORO: Validar fronteras de Clean/Hexagonal Architecture
    'boundaries/element-types': [
      'error',
      {
        default: 'disallow',
        message: 'Violación Arquitectónica: Capa "${from}" no puede depender de "${to}". Respeta las fronteras de Clean/Hexagonal.',
        rules: [
          {
            from: 'core',
            allow: ['core']
          },
          {
            from: 'application',
            allow: ['core', 'application']
          },
          {
            from: 'infrastructure',
            allow: ['core', 'application', 'infrastructure']
          }
        ]
      }
    ],

    // 1. Prohibir el uso de 'any' de forma estricta (OWASP / TypeScript safety)
    '@typescript-eslint/no-explicit-any': 'error',
    '@typescript-eslint/no-unsafe-assignment': 'error',
    '@typescript-eslint/no-unsafe-member-access': 'error',
    '@typescript-eslint/no-unsafe-call': 'error',
    '@typescript-eslint/no-unsafe-return': 'error',

    // 2. Forzar el uso de 'interfaces' sobre 'types' para definir estructuras de objetos
    '@typescript-eslint/consistent-type-definitions': ['error', 'interface'],

    // 3. Estándares adicionales de alta calidad
    '@typescript-eslint/explicit-function-return-type': 'error',
    '@typescript-eslint/explicit-module-boundary-types': 'error',
    '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
    'no-console': ['warn', { allow: ['warn', 'error', 'info'] }],
  },
};
