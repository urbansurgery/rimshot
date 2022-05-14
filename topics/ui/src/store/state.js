import firebase from 'firebase/app';
import 'firebase/firestore';

export default {
  views: [],
  issues: [],
  workshops: [],
  projects: [],
  activeIssue: null,
  activeWorkshop: null,
  activeProject: null,
  isEmbedded: false,
  isDevMode: false,
  currentWorkshop: undefined,
  currentProject: undefined,
};
