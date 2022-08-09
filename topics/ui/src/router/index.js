import Vue from 'vue';
import VueRouter from 'vue-router';

Vue.use(VueRouter);

const branchRoute = (name) => (to, _from, next) => {
  const { query, params } = to;

  if (window.UIBindings || query.pane) {
    return next({ name, query, params });
  }

  return next();
};

const embeddedSpeckleCheck = (to, _from, next) => {
  console.log(arguments);

  const { query } = to;

  if (window.UIBindings || query.pane) {
    console.log(window.UIBindings || 'No Bindings');
    return next();
  }

  return next('/');
};

const routes = [
  {
    path: '/',
    name: 'Root',
    component: function () {
      return import('../views/Blank.vue');
    },
  },
  {
    path: '/about',
    name: 'About',
    component: function () {
      return import('../views/About.vue');
    },
  },
  {
    path: '/speckleui',
    name: 'SpeckleUI',
    component: function () {
      return import('../views/SpeckleUI.vue');
    },
    meta: { title: 'Speckle' },
    beforeEnter: (to, _from, next) => {
      const { query } = to;

      if (window.UIBindings || query.pane) {
        if (!window.UIBindings) {
          console.log('%c%s: No Bindings!', 'color: hotpink', 'SpeckleRoamer'); //TODO add dummy bindings for testing
        } else {
          console.log(
            '%c%s: Bindings Attached!',
            'color: hotpink',
            'SpeckleRoamer'
          );
        }
        return next();
      }

      return next('/');
    },
  },
  {
    path: '/issues',
    component: function () {
      return import('../views/List.vue');
    },
    children: [
      {
        name: 'issue-list',
        path: '',
        component: function () {
          return import('../views/Web.vue');
        },
        beforeEnter: branchRoute('issue-list-pane'),
        meta: { title: 'Issue List' },
      },
      {
        name: 'issue-list-pane',
        path: '',
        component: function () {
          return import('../views/Pane.vue');
        },
        meta: { title: 'Issue List' },
      },
      {
        path: ':workshopId',
        name: 'workshop',
        beforeEnter: branchRoute('workshop-pane'),
        component: function () {
          return import('../views/Web.vue');
        },
        meta: { title: 'Workshop' },
      },
      {
        path: ':workshopId',
        name: 'workshop-pane',
        component: function () {
          return import('../views/Pane.vue');
        },
        meta: { title: 'Workshop' },
      },
    ],
  },
];

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes,
});

const DEFAULT_TITLE = 'Rimshot - Realtime Issue Management with Speckle';
router.afterEach((to, from) => {
  // Use next tick to handle router history correctly
  // see: https://github.com/vuejs/vue-router/issues/914#issuecomment-384477609
  Vue.nextTick(() => {
    document.title = to.meta.title || DEFAULT_TITLE;
  });
});

export default router;
