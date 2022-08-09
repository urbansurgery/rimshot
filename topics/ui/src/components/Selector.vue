<script>
  import Vue from 'vue';

  import { mapGetters, mapState } from 'vuex';
  import { sortByKey } from '@/utilities/utils';
  import { isoDate } from '@/filters';

  export default Vue.extend({
    name: 'WorkshopSelector',
    computed: {
      ...mapGetters(['liveProjects']),
      ...mapState({
        isDevMode: (state) => state.isDevMode,
        isEmbedded: (state) => state.isEmbedded,
        projects: (state) => state.projects,
        workshops: (state) => state.workshops,
        selectedIssue: (state) => state.activeIssue,
      }),
      route() {
        if (this.$route && this.$route.meta) return this.$route.meta.title;
        return '';
      },
      projectList() {
        return sortByKey(this.liveProjects, 'key', 'asc');
      },
      workshopLink() {
        return this.workshops.filter(
          (workshop) => workshop.id === this.selectedIssue.id
        )[0];
      },
      projectWorkshops() {
        const workshops = this.workshops;
        return workshops.filter(
          (workshop) => workshop?.project?.key === this.activeProject.key
        );
      },
      workshopList() {
        if (this.selectedProject && this.projectWorkshops) {
          return sortByKey(this.projectWorkshops, 'number', 'desc');
        }
        return [];
      },
      selectedWorkshop: {
        get() {
          let workshopId = this.$store.state.activeWorkshop;
          return workshopId;
        },
        set(workshopId) {
          this.$store.commit('SET_SELECTED_WORKSHOP', workshopId);
        },
      },
      selectedProject: {
        get() {
          return this.$store.state.activeProject;
        },
        set(projectId) {
          this.$store.commit('SET_SELECTED_PROJECT', projectId);
        },
      },
      activeProject() {
        return this.liveProjects.find(
          (project) => project.id === this.selectedProject
        );
      },
    },
    mounted: function () {},
    methods: {
      workshopChange(value) {
        if (!value || !this.selectedProject) return;
        if (value === this.selectedProject) {
          if (this.projectWorkshops.length === 1) {
            this.selectedWorkshop = this.projectWorkshops[0].id;
          }
          if (this.projectWorkshops.length > 1) {
            this.selectedWorkshop = sortByKey(
              this.projectWorkshops,
              'number',
              'desc'
            )[0].id;
          }
          console.log('changed project');
        } else {
          console.log('changed workshop');
        }
      },
      isoDate(date) {
        return isoDate(date);
      },
    },
  });
</script>

<template>
  <div id="workshop-selector">
    <v-container fluid>
      <v-row
        v-if="(isEmbedded || isDevMode) && route != 'Workshop'"
        dense
        class="pt-2"
      >
        <v-col>
          <v-select
            v-model="selectedProject"
            dense
            label="Project"
            :items="projectList"
            item-text="name"
            item-value="id"
            @change="workshopChange"
            ><template #item="{ item }"
              ><span>{{ item.key }} - {{ item.name }}</span></template
            >
          </v-select></v-col
        ><v-col>
          <v-select
            v-model="selectedWorkshop"
            :disabled="!selectedProject"
            dense
            label="Workshop"
            :items="workshopList"
            :item-text="
              (item) =>
                `${item.number} [${isoDate(item.meetingDate)}] - ${
                  item.summary
                }`
            "
            item-value="id"
            @change="workshopChange"
          >
            <template #item="{ item }">
              <span>
                {{ item.number }} [{{ item.meetingDate | isoDate }}] -
                {{ item.summary }}
              </span>
            </template></v-select
          ></v-col
        ></v-row
      ></v-container
    >
  </div>
</template>
