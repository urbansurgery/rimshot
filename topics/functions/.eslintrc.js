module.exports = {
  root: true,
  env: {
    es6: true,
    node: true,
  },
  extends: [
    "../../configuration/esLintDefaults.json",
    "eslint:recommended",
    "plugin:import/errors",
    "plugin:import/warnings",
    "plugin:import/typescript",
    "google",
    "plugin:@typescript-eslint/recommended",
  ],
  // parser: "@typescript-eslint/parser",
  parserOptions: {
    // project: ["tsconfig.json"],
    sourceType: "module",
  },
  ignorePatterns: [
    "/lib/**/*", // Ignore built files.
  ],
  plugins: ["@typescript-eslint", "import"],
  rules: {
    "quotes": ["error", "double"],
    "import/no-unresolved": 0,
  },
};
