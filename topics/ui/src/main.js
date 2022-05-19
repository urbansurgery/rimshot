import Vue from 'vue';
import App from './App.vue';
import './registerServiceWorker';
import router from './router';
import { store } from './store';
import vuetify from './plugins/vuetify';

Vue.config.productionTip = false;

// Filters
import { snakeCaseToTitleCase, isoDate, trim, leftPad } from './filters';

Vue.filter('snakeCaseToTitleCase', snakeCaseToTitleCase);
Vue.filter('isoDate', isoDate);
Vue.filter('trim', trim);
Vue.filter('leftPad', leftPad);

new Vue({
  vuetify,
  router,
  store,
  beforeCreate() {
    this.$store.commit('initialiseStore');
  },
  render: (h) => h(App),
}).$mount('#app');
