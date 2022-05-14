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

const routes = [
  {
    path: '/',
    name: 'Root',
    component: function () {
      return import('../views/Blank.vue');
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

export default router;
