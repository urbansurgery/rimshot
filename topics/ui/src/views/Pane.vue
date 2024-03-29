<script>
  import Vue from 'vue';
  import { mapGetters, mapState } from 'vuex';
  import { sortByKey } from '@/utilities/utils';

  import IssuesGrid from '@/components/Issues.vue';
  import WorkshopSelector from '@/components/Selector.vue';
  import { templates } from '@/store/state';
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
        isEmbedded: (state) => state.isEmbedded,
        issues: (state) => state.issues,
        currentWorkshop: (state) => state.currentWorkshop,
        currentProject: (state) => state.currentProject,
      }),
      ...mapGetters(['liveProjects']),
      workshopList() {
        if (this.selectedProject) {
          return sortByKey(this.projectWorkshops, 'number', 'desc');
        }
        return [];
      },
      projectWorkshops() {
        if (this.selectedProject) {
          return this.workshops.filter(
            (workshop) => workshop?.project?.key === this.currentProject.key
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
      nextIssue() {
        let number = this.issues.length + 1;
        const issue = {
          ...templates.issue,
          number,
          workshopId: this.selectedWorkshop,
          projectId: this.selectedProject,
          deleted: false,
        };

        Object.keys(issue).forEach((key) => {
          if (issue[key] === undefined) {
            delete issue[key];
          }
        });

        return issue;
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
          await navigator.clipboard.writeText(link.href);
          this.snackbar.copy = true;
        }
      },
      async addIssue(e) {
        e.stopPropagation();
        console.log(e);

        const newIssue = await this.$store.dispatch('ADD_ISSUE', {
          issue: this.nextIssue,
        });

        if (!newIssue) return (this.selectedIssue = undefined);

        const { id: issueId, index } = newIssue;

        if (issueId === this.selectedIssue) {
          this.selectedIssue = undefined;
        } else {
          this.$store.commit('SET_SELECTED_ISSUE', issueId);
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
      <v-btn
        bottom
        right
        fab
        color="complementary"
        style="position: fixed; bottom: 4rem; right: 2rem; z-index: 100"
        @click="addIssue"
      >
        <v-icon>mdi-plus</v-icon></v-btn
      >
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
