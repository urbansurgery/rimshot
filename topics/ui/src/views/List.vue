<script>
  // @ is an alias to /src
  import Vue from 'vue';
  import IssueList from '@/components/IssueList.vue';
  import { mapActions, mapState, Store } from 'vuex';

  function parseJson(json) {
    try {
      return JSON.parse(json);
    } catch (_e) {
      return json;
      s;
    }
  }

  export default {
    name: 'ListView',
    components: {},
    data: () => ({
      selectedWorkshopDetails: undefined,
    }),
    computed: {
      ...mapState({
        activeWorkshop: (state) => state.activeWorkshop,
        projects: (state) => state.projects,
        workshops: (state) => state.workshops,
        packageVersion: (state) => state.packageVersion,
        activeIssue: (state) => state.activeIssue,
      }),
      isEmbedded() {
        return this.$store.state.isEmbedded;
      },
      route() {
        return this.$route.name;
      },
      title() {
        if (this.$route && this.$route.meta) return this.$route.meta.title;
        return '';
      },
      hosted() {
        return this.isEmbedded ? 'Hosted in Navisworks' : 'Shown in Browser';
      },
      bound() {
        return this.projects.length > 0 && this.workshops.length > 0;
      },
      issueProps() {
        return { embedded: this.isEmbedded };
      },
      paneView() {
        return Boolean(window.UIBindings);
      },
    },
    watch: {
      $progress(to, from) {
        console.log(to);
      },
      bound(bound) {
        if (bound && this.activeWorkshop) {
          this.selectedWorkshopDetails = this.$store.getters[
            'singleWorkshopDetails'
          ](this.activeWorkshop);
        }
      },
    },
    created() {
      window.Store = this.$store;
      window.EventBus = new Vue();
      window.app = this;

      window.EventBus.$on('element-progress', (payload) => {
        if (payload) {
          const { current, count } = parseJson(payload);
          // console.log({ elements: [current, count] });
          this.$store.commit('SET_ELEMENT_PROGRESS', current / count);
        }
      });

      window.EventBus.$on('nested-progress', (payload) => {
        if (payload) {
          const { current, count } = parseJson(payload);
          // console.log({ nested: payload });
          this.$store.commit('SET_NESTED_PROGRESS', current / count);
        }
      });

      window.EventBus.$on('geometry-progress', (payload) => {
        if (payload) {
          const { current, count } = parseJson(payload);
          // console.log({ geometry: payload });
          this.$store.commit('SET_GEOMETRY_PROGRESS', current / count);
        }
      });

      window.EventBus.$on('commit_sent', (commit_payload) => {
        if (commit_payload) {
          const { streamId, issueId, commitId, objectId } =
            parseJson(commit_payload);

          if (streamId && issueId && commitId && objectId) {
            this.$store
              .dispatch('RECORD_ISSUE_COMMIT', {
                issueId,
                commitId,
                streamId,
                objectId,
              })
              .then(() => {
                this.$store.commit('CANCEL_COMMIT_PROGRESS', { issueId });
              });
          } else if (!commitId) {
            // TODO: handle the non-selected objects case.
            this.$store.commit('CANCEL_COMMIT_PROGRESS', { issueId });
          }
        }
      });

      window.EventBus.$on('new-image', (imagejson) => {
        if (imagejson) {
          const image = JSON.parse(imagejson);
          const view = {
            guid: image.guid,
            name: image.name,
            workshopId: this.activeWorkshop,
            image: image.image,
            thumbnail: image.thumbnail,
          };

          let viewpoint;

          console.log({ image });

          if (image.viewpoint) {
            const {
              CameraType,
              FieldOfView,
              AspectRatio,
              ViewToWorldScale,
              ...camera
            } = image.viewpoint;

            if (CameraType === 'OrthogonalCamera') {
              viewpoint = { ...camera, $type: CameraType, ViewToWorldScale };
            }
            if (CameraType === 'PerspectiveCamera') {
              viewpoint = {
                ...camera,
                $type: CameraType,
                FieldOfView,
                AspectRatio,
              };
            }

            view.viewpoint = viewpoint;
            view.viewpoint && (view.viewpoint.$type = CameraType);
          }

          window.Store.dispatch('ADD_VIEW', {
            view,
            workshopId: this.activeWorkshop,
            issueId: this.activeIssue,
          });
        }
      });
    },
    mounted: async function () {
      this.Bindings = window.UIBindings;
      const projects = await this.bindProjects();
      const workshops = await this.bindWorkshops();
      // console.log({ projects, workshops });
    },
    methods: {
      ...mapActions(['bindProjects', 'bindWorkshops']),
      refresh() {
        this.Bindings.refresh();
      },
      showDevTools() {
        this.Bindings.showDev();
      },
      addImage() {
        this.Bindings.addImage();
      },
      darkMode() {
        this.$vuetify.theme.dark = !this.$vuetify.theme.dark;
      },
    },
  };
</script>

<template>
  <div id="issue-list">
    <v-app :class="{ hosted: paneView }">
      <v-app-bar class="elevation-2" dense app flat>
        <v-toolbar-title class="primary--text">
          <span class="host">{{ hosted }}</span>
          <span v-if="title == 'Workshop'" class="view">: Workshop View"</span>
          <span v-else class="view">: Analyst View</span>
          <span
            v-if="bound && selectedWorkshopDetails"
            class="workshop"
            style="margin-left: 1ex"
            ><span class="in">&nbsp;=>&nbsp;</span
            >{{
              selectedWorkshopDetails && selectedWorkshopDetails.project.name
            }}
            : Workshop
            {{
              selectedWorkshopDetails && selectedWorkshopDetails.workshop.number
            }}</span
          >
        </v-toolbar-title>
      </v-app-bar>

      <v-main>
        <router-view :embedded="isEmbedded" />
      </v-main>

      <v-footer fixed>
        <v-row>
          <v-col>
            <v-btn v-if="isEmbedded" x-small text @click="addImage"
              >Snapshot</v-btn
            ></v-col
          >
          <v-col
            ><v-btn v-if="isEmbedded" x-small text @click="refresh"
              >Refresh</v-btn
            ></v-col
          >
          <v-col>
            <v-btn v-if="isEmbedded" x-small text @click="showDevTools"
              >Devtools</v-btn
            >
          </v-col>
          <v-col>
            <v-btn x-small text @click="darkMode">
              {{ $vuetify.theme.dark ? 'Light' : 'Dark' }}
            </v-btn>
          </v-col>
        </v-row>
      </v-footer>
    </v-app>
  </div>
</template>

<style lang="scss" scoped>
  footer {
    background-color: var(--v-dark-red);
  }
  #issue-list {
    margin-bottom: 12px;
  }
  .hosted {
    zoom: 85%;
  }
</style>
