const webpack = require('webpack');
const path = require('path');
// webpack configuration file

const globalSassFiles = ['~@/theme/_variables.scss'];

module.exports = {
  transpileDependencies: ['vuetify'],
  outputDir: './dist',
  devServer: {
    port: 8080,
  },
  publicPath: '/',
  pluginOptions: {
    webpackBundleAnalyzer: {
      analyzerMode: 'disabled',
    },
  },
  configureWebpack: (config) => {
    console.log({ env: process.env.NODE_ENV });

    if (process.env.NODE_ENV === 'development') {
      config.devtool = 'source-map';

      config.output.devtoolModuleFilenameTemplate = (info) =>
        info.resourcePath.match(/^\.\/\S*?\.vue$/)
          ? `webpack-generated:///${info.resourcePath}?${info.hash}`
          : `webpack-yourCode:///${info.resourcePath}`;

      config.output.devtoolFallbackModuleFilenameTemplate =
        'webpack:///[resource-path]?[hash]';
    }

    config.plugins = [
      ...config.plugins, // this is important !
      new webpack.DefinePlugin({}),
    ];

    config.module.rules.push({
      test: /\.scss$/,
      use: [
        'sass-loader',
        {
          loader: 'sass-resources-loader',
          options: {
            resources: [
              path.resolve(__dirname, './src/theme/_variables.scss'), // global variables
            ],
          },
        },
      ],
    });
  },
  chainWebpack: (config) => {
    config.plugin('html').tap((args) => {
      args[0].title = `Rimshot - Realtime Issue Management with Speckle`;
      return args;
    });
  },
  pwa: {
    short_name: `Rimshot`,
    name: `Rimshot - Realtime Issue Management with Speckle`,
    themeColor: '#ffffff',
    msTileColor: '#000000',
    appleMobileWebAppCapable: 'yes',
    appleMobileWebAppStatusBarStyle: '#7f0000',
    display: 'minimal-ui',
    scope: '/',

    shortcuts: [],

    // configure the workbox plugin
    workboxPluginMode: 'GenerateSW',
    workboxOptions: {
      // swSrc is required in InjectManifest mode.
      //   swSrc: "dev/sw.js",
      // ...other Workbox options...
    },
  },
  css: {
    sourceMap: true,
    loaderOptions: {
      sass: {
        // Here we can specify the older indent syntax formatted SASS
        // Note that there is *not* a semicolon at the end of the below line
        additionalData: globalSassFiles
          .map((src) => '@import "' + src + '"')
          .join('\n'),
      },
      scss: {
        additionalData: globalSassFiles
          .map((src) => '@import "' + src + '";')
          .join('\n'),
      },
    },
  },
};
