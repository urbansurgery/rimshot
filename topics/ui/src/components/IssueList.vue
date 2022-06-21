<script>
  import Vue from 'vue';
  import { db } from '@/plugins/firebase';
  import { mapActions, mapState, Store } from 'vuex';
  import IssueBlock from '@/components/IssueBlock.vue';
  export default {
    name: 'IssueList',
    components: { 'issue-block': IssueBlock },
    data: () => ({
      issues: [
        {
          key: 'XYZ-1',
          title: 'Issue 1',
          description: 'This is issue 1',
          workshop: '1',
        },
      ],
    }),
    computed: {
      ...mapState({
        activeWorkshop: (state) => state.activeWorkshop,
        projects: (state) => state.projects,
        workshops: (state) => state.workshops,
        packageVersion: (state) => state.packageVersion,
        activeIssue: (state) => state.activeIssue,
        isEmbedded: (state) => state.isEmbedded,
      }),
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
    },
    watch: {
      bound(bound) {
        if (bound && this.activeWorkshop) {
          this.selectedWorkshopDetails = this.$store.getters[
            'singleWorkshopDetails'
          ](this.activeWorkshop);
        }
      },
    },
    mounted: async function () {
      window.name = 'issue-list';
      await this.bindProjects();
      await this.bindWorkshops();
    },
    created: {},
    methods: {
      ...mapActions(['bindProjects', 'bindWorkshops']),
    },
  };
</script>

<template>
  <v-container dense class="pt-0 list pl-6 issue-list">
    <v-row v-for="issue in issues" :key="issue.id">
      <v-col><issue-block :issue="issue" /></v-col
    ></v-row>
  </v-container>
</template>

<style lang="scss" scoped>
  .issue-list {
    margin-top: 66px;
  }
</style>
