<script>
  import Vue from 'vue';
  import { mapGetters, mapState } from 'vuex';
  import { sortByKey } from '@/utilities/utils';

  import IssuesGrid from '@/components/Issues.vue';
  import WorkshopSelector from '@/components/Selector.vue';

  export default Vue.extend({
    name: 'PaneView',
    components: {
      'issues-grid': IssuesGrid,
      'workshop-selector': WorkshopSelector,
    },
    data: () => ({
      project: {
        name: '',
        key: '',
        live: true,
        archived: false,
        id: '',
        roles: [],
      },
      snackbar: { copy: false },
    }),
    computed: {
      ...mapState({
        projects: (state) => state.projects,
        workshops: (state) => state.workshops,
        selectedIssue: (state) => state.activeIssue,
      }),
      ...mapGetters(['liveProjects', 'selectedWorkshops']),
      workshopList() {
        if (this.selectedProject) {
          return sortByKey(
            this.selectedWorkshops(this.selectedProject),
            'number',
            'desc'
          );
        }
        return [];
      },
      workshopLink() {
        const workshops = this.workshops.filter(
          (workshop) => this.selectedIssue?.id === workshop.id
        );

        return workshops[0]?.link;
      },
      isEmbedded() {
        return Boolean(window.UIBindings) || this.$route.query.pane === true;
      },
      isDevMode() {
        return Boolean(Vue.config.devtools);
      },
      route() {
        if (this.$route && this.$route.meta) return this.$route.meta.title;
        return '';
      },
      href() {
        return window.location.href;
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
    watch: {},
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
      async copyHrefToClipboard(e) {
        e.stopPropagation();

        if (e.target) {
          var link = e.target;

          const textArea = document.createElement('textarea');
          textArea.value = link.href;

          document.body.appendChild(textArea);
          textArea.focus();
          textArea.select();

          document.execCommand('copy');
          textArea.remove();
          this.snackbar.copy = true;
        }
      },
    },
  });
</script>

<template>
  <div id="pane">
    <v-container fluid class="px-1 py-0">
      <workshop-selector />
      <issues-grid
        v-if="selectedWorkshop"
        :workshop-id="selectedWorkshop"
        :project-id="selectedProject"
        :embedded="isEmbedded"
      />
      <v-row class="pa-6 mt-0 text-caption link-management"
        ><v-col
          v-if="selectedWorkshop && (isEmbedded || isDevMode) && workshopLink"
          ><a
            :href="`https://grfn.io/analysis/register/${workshopLink.project.key}/${workshopLink.number}`"
            target="analysis-register"
            >View in Analysis Register</a
          ></v-col
        ><v-col
          ><span
            v-if="
              selectedWorkshop &&
              (isEmbedded || isDevMode) &&
              (route != 'Workshop' || isEmbedded)
            "
            class="attendees-view"
          >
            Attendees View:
            <a
              :href="`/issues/${selectedWorkshop}`"
              @click.prevent="copyHrefToClipboard"
              >Copy</a
            >
          </span>
          <span>
            <router-link
              v-if="route != 'Workshop' && selectedProject && selectedWorkshop"
              class="lock"
              :to="{
                path: `/issues/${selectedWorkshop}`,
                params: { workshopId: selectedWorkshop },
                query: $route.query,
              }"
              >Lock</router-link
            >
          </span></v-col
        >
      </v-row>
    </v-container>
    <v-snackbar v-model="snackbar.copy" :timeout="2000" rounded="pill">
      Attendees View link copied to clipboard.
      <template #action="{ attrs }">
        <v-btn color="pink" text v-bind="attrs" @click="snackbar.copy = false">
          Close
        </v-btn>
      </template>
    </v-snackbar>
  </div>
</template>

<style lang="scss" scoped>
  .attendees-view {
    padding-right: 1ex;
  }

  .link-management {
    margin-bottom: 48px;
  }

  .hosted {
    zoom: 70%;
  }
</style>
