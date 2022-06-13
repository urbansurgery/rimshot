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

const templates = {
  project: {
    archived: false,
    host: 'speckle.xyz',
    key: 'XXX',
    live: false,
    name: 'Temnplate Project',
    speckle_stream: undefined,
  },
  workshop: {
    archived: false,
    number: '0',
    meetingDate: new Date(),
    project: {
      key: 'XXX',
    },
    summary: '',
  },
  issue: {
    actionRequired: '',
    created: new Date(),
    deleted: false,
    number: undefined,
    speckle_commit: undefined,
    speckle_commit_object: undefined,
    speckle_stream: undefined,
    summary: '',
    workshopId: undefined,
  },
};
