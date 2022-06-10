<script>
  import update from './mixins/update';
  export default {
    name: 'App',
    mixins: [update],
    data: () => ({}),
    computed: {
      route() {
        return this.$route;
      },
      theme() {
        return this.$vuetify.theme.dark ? 'dark' : 'light';
      },
    },
    watch: {
      route(to) {
        this.$store.commit(
          'SET_EMBEDDED',
          Boolean(window.UIBindings) || to.query.pane
        );
      },
    },
    mounted() {
      this.$store.commit(
        'SET_DEV_MODE',
        process.env.NODE_ENV === 'development'
      );
    },
  };
</script>

<template>
  <div id="app">
    <v-app
      dark
      :style="{ background: $vuetify.theme.themes[theme].background }"
    >
      <router-view />
    </v-app>
    <v-snackbar bottom right :value="updateExists" :timeout="0" color="primary">
      An update is available
      <v-btn text @click="refreshApp"> Update </v-btn>
    </v-snackbar>
  </div>
</template>

<style lang="scss">
  body {
    margin: 0;
    overflow-y: hidden;
  }

  #app {
    background-color: var(--v-background-lighten1);
    font-family: Space Grotesk, sans-serif; /* 1 */

    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
  }

  html {
    overflow-y: auto !important;
  }
</style>
