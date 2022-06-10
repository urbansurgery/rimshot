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
    themeColor: '#047efb',
    msTileColor: '#2f2f2f',
    appleMobileWebAppCapable: 'yes',
    appleMobileWebAppStatusBarStyle: '#047efb',
    display: 'minimal-ui',

    manifestOptions: {},

    iconPaths: {
      favicon: 'img/icons/favicon.ico',
      favicon32: 'img/icons/favicon-32x32.png',
      favicon16: 'img/icons/favicon-16x16.png',
      appleTouchIcon: 'img/icons/apple-touch-icon-152x152.png',
      'android-chrome-192x192': 'img/icons/android-chrome-192x192.png',
    },

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
