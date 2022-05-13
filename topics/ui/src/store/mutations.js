import { vuexfireMutations } from 'vuexfire';

export default {
  ...vuexfireMutations,
  initialiseStore({ state, commit }) {},
  SET_APP_VERSION: (state, version) => {
    state.packageVersion = version;
  },
  SET_SELECTED_PROJECT: (state, project) => {
    state.activeProject = project;
  },
  SET_SELECTED_WORKSHOP: (state, workshop) => {
    state.activeWorkshop = workshop;
  },
  SET_SELECTED_ISSUE: (state, issue) => {
    state.activeIssue = issue;
  },
  SET_EMBEDDED: (state, flag) => {
    state.isEmbedded = Boolean(flag);
  },
  SET_DEV_MODE: (state, flag) => {
    state.isDevMode = Boolean(flag);
  },
  RESET_STATE: () => {},
};
