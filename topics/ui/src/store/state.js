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
  commitProgress: false,
  commitElements: 0,
  commitNested: 0,
  commitGeometry: 0,
};
