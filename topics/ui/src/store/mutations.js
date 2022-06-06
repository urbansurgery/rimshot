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
  SET_ISSUE_COMMIT: (state, { commitId, stream, issueId }) => {},
  SET_COMMIT_PROGRESS: (state, flag) => {
    if (flag === Boolean(flag)) {
      state.commitProgress = Boolean(flag);
    } else {
      state.commitProgress = flag;
    }
  },
  SET_ELEMENT_PROGRESS: (state, progress) => {
    const { count, current } = JSON.parse(progress);
    state.commitElements = count == 0 ? 0 : Math.round((current / count) * 100);
    // console.log(state.commitGeometry, progress);
  },
  SET_NESTED_PROGRESS: (state, progress) => {
    const { count, current } = JSON.parse(progress);
    state.commitNested = count == 0 ? 0 : Math.round((current / count) * 100);
    // console.log(state.commitGeometry, progress);
  },
  SET_GEOMETRY_PROGRESS: (state, progress) => {
    const { count, current } = JSON.parse(progress);
    state.commitGeometry = count == 0 ? 0 : Math.round((current / count) * 100);
    // console.log(state.commitGeometry, progress);
  },
  CANCEL_COMMIT_PROGRESS: (state, { issueId }) => {
    // console.log('Cancelling commit progress');
    if (issueId === state.commitProgress) {
      state.commitProgress = false;
      state.commitGeometry = 0;
      state.commitNested = 0;
      state.commitElements = 0;
    }
  },
};
