import Vue from 'vue';
import Vuex from 'vuex';

import { db } from '@/plugins/firebase';

import createPersistedState from 'vuex-persist-indexeddb';

import mutations from './mutations';
import actions from './actions';
import state from './state';
import getters from './getters';

Vue.use(Vuex);

const debug = process.env.NODE_ENV !== 'production';

export const store = new Vuex.Store({
  state,
  getters,
  actions,
  mutations,
  modules: {},
  strict: debug,
  plugins: [
    createPersistedState({
      key: 'rimshot',
      paths: ['activeWorkshop', 'activeProject', 'activeIssue'],
    }),
  ],
});
