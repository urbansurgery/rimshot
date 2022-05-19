<script>
  import Vue from 'vue';
  import { mapState, Store, mapActions } from 'vuex';
  import { db } from '@/plugins/firebase';

  import Issues from '../components/Issues.vue';

  export default Vue.extend({
    name: 'WebView',
    components: { Issues },

    data: () => ({
      selectedWorkshopDetails: undefined,
      project: {
        name: '',
        code: '',
        key: '',
        live: true,
        archived: false,
        id: '',
        roles: [],
      },
    }),
    computed: {
      ...mapState({
        projects: (state) => state.projects,
        workshops: (state) => state.workshops,
        packageVersion: (state) => state.packageVersion,
        activeIssue: (state) => state.activeIssue,
        activeWorkshop: (state) => state.activeWorkshop,
      }),
      hosted() {
        return window.UIBindings ? 'Hosted in Navisworks' : 'Shown in Browser';
      },
      isEmbedded() {
        return Boolean(window.UIBindings) || this.$route.query.pane === 'true';
      },
      route() {
        return this.$route.name;
      },
      bound() {
        return this.projects.length > 0 && this.workshops.length > 0;
      },
      selectedWorkshop: {
        get() {
          return this.$store.state.activeWorkshop;
        },
        set(workshop) {
          this.$store.commit('SET_SELECTED_WORKSHOP', workshop);
        },
      },
      selectedProject: {
        get() {
          return this.$store.state.activeProject;
        },
        set(project) {
          this.$store.commit('SET_SELECTED_PROJECT', project);
        },
      },
    },
    beforeCreate: function () {
      window.Db = db;
    },
    mounted: function () {
      if (this.$route.meta && this.$route.meta.title === 'Workshop') {
        this.selectedWorkshop = this.$route.params.workshopId;

        const sw = this.workshops.filter(
          (w) => w.id === this.selectedWorkshop
        )[0];

        if (sw && sw.project && sw.project.key) {
          const p = this.projects.filter((p) => p.key === sw.project.key)[0];

          if (p) this.project = p;
        }
      }
    },

    methods: {
      ...mapActions(['bindReference']),
    },
  });
</script>

<template>
  <div id="web">
    <v-container fluid>
      <issues
        v-if="selectedWorkshop"
        :workshop-id="selectedWorkshop"
        :embedded="isEmbedded"
      />
    </v-container>
  </div>
</template>

<style lang="scss">
  .v-footer {
    justify-items: flex-end;
    flex-wrap: nowrap;
  }

  .hosted {
    zoom: 70%;
  }

  .hosted .v-main {
    margin-bottom: 2em;
  }
</style>
