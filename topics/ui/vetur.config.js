// vetur.config.js
/** @type {import('vls').VeturConfig} */
module.exports = {
  settings: {
    'vetur.useWorkspaceDependencies': true,
    'vetur.experimental.templateInterpolationService': true,
    'vetur.validation.interpolation': true,
  },
  projects: [
    './ui',
    {
      root: './ui',
      package: './package.json',
      tsconfig: '/tsconfig.json',
      globalComponents: ['./src/components/**/*.vue'],
    },
  ],
};
