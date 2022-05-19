import Vue from 'vue';
import Vuetify from 'vuetify/lib';

// // Ensure you are using css-loader
// import 'material-design-icons-iconfont/dist/material-design-icons.css';
// import '@mdi/font/css/materialdesignicons.css';

Vuetify.config.silent = true;

Vue.use(Vuetify);

import theme from '../theme';

export default new Vuetify({
  theme,
  // iconfont: 'mdi',
  // icons: {},
});
