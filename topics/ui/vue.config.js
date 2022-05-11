const webpack = require('webpack');

const globalSassFiles = ['~@/theme/_variables.scss'];

module.exports = {
  transpileDependencies: ['vuetify'],
  outputDir: './dist',
  devServer: {
    port: 8080,
  },
  publicPath: '/',
  chainWebpack: (config) => {
    config.plugin('html').tap((args) => {
      args[0].title = 'GRFN Embedded Tools';
      return args;
    });
    config.devtool('inline-source-map');
    ['vue-modules', 'vue', 'normal-modules', 'normal'].forEach((match) => {
      config.module
        .rule('sass')
        .oneOf(match)
        .use('sass-loader')
        .tap((opt) => {
          return Object.assign(opt, {
            additionalData: globalSassFiles
              .map((src) => '@import "' + src + '"')
              .join('\n'),
          });
        });
    });
  },
  configureWebpack: (config) => {
    config.plugins = [
      ...config.plugins, // this is important !
      // new webpack.DefinePlugin({
      //   'process.env': { PACKAGE_VERSION: `"${version}"` },
      // }),
      new webpack.LoaderOptionsPlugin({
        debug: true,
      }),
    ];
    config.optimization = {
      ...config.optimization,
      runtimeChunk: 'single',
      splitChunks: {
        minSize: 10000,
        maxSize: 250000,
      },
    };
    config.performance = {
      ...config.performance,
      hints: false,
    };
  },
  css: {
    sourceMap: true,
    loaderOptions: {
      sass: {
        additionalData: globalSassFiles
          .map((src) => '@import "' + src + '";')
          .join('\n'),
      },
    },
  },
};
