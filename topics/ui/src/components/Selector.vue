<script>
  import Vue from 'vue';

  import { mapGetters, mapState } from 'vuex';
  import { sortByKey } from '@/utilities/utils';

  export default Vue.extend({
    name: 'WorkshopSelector',
    computed: {
      ...mapGetters(['liveProjects', 'selectedWorkshops']),
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
          /> </v-col
        ><v-col>
          <v-select
            v-model="selectedWorkshop"
            :disabled="!selectedProject"
            dense
            label="Workshop"
            :items="workshopList"
            item-text="number"
            item-value="id"
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
