import firebase from 'firebase/app';
import 'firebase/firestore';
import { FirestoreOptions } from '@posva/vuefire-core';
import { firestoreAction } from 'vuexfire';
import { db } from '@/plugins/firebase';

export default {
  bindProjects: firestoreAction(({ bindFirestoreRef }) => {
    // return the promise returned by `bindFirestoreRef`
    return bindFirestoreRef(
      'projects',
      db.collection('projects').where('archived', '==', false)
    );
  }),
  bindWorkshops: firestoreAction(({ bindFirestoreRef }) => {
    return bindFirestoreRef(
      'workshops',
      db.collection('workshops').where('archived', '==', false)
    );
  }),
  bindCurrentWorkshop: firestoreAction(({ bindFirestoreRef }, workshopId) => {
    return bindFirestoreRef(
      'currentWorkshop',
      db.collection('workshops').doc(workshopId)
    );
  }),
  bindCurrentProject: firestoreAction(({ bindFirestoreRef }, projectId) => {
    return bindFirestoreRef(
      'currentProject',
      db.collection('projects').doc(projectId)
    );
  }),
  bindWorkshopViews: firestoreAction(({ bindFirestoreRef }, workshopId) => {
    return bindFirestoreRef(
      'views',
      db
        .collection('views')
        .where('workshopId', '==', workshopId)
        .where('deleted', '==', false)
    );
  }),
  bindWorkshopViewsByQuery: firestoreAction(({ bindFirestoreRef }, query) => {
    return bindFirestoreRef('views', query);
  }),
  bindWorkshopIssues: firestoreAction(({ bindFirestoreRef }, workshopId) => {
    return bindFirestoreRef(
      'issues',
      db
        .collection('issues')
        .where('workshopId', '==', workshopId)
        .where('deleted', '==', false)
    );
  }),

  ADD_VIEW: async (
    { state, dispatch, commit },
    { view, workshopId, issueId }
  ) => {
    const now = firebase.firestore.Timestamp.fromDate(new Date());
    let newIssue;

    if (!workshopId) throw 'No workshop selected';
    if (!issueId) {
      const issueNumbers = state.issue
        ?.map((issue) => issue.number)
        .reduce((max, num) => {
          return max > num ? max : num;
        }, 0);

      newIssue = await dispatch('ADD_ISSUE', {
        workshopId,
        number: issueNumbers + 1,
        created: now,
        deleted: false,
      });

      issueId = newIssue.id;
    }

    // validate viewpoint
    if (view.viewpoint) {
      const viewpoint = view.viewpoint;
      const vp = {
        $type: viewpoint.$type,
        CameraViewpoint: {
          X: viewpoint.CameraViewpoint.X,
          Y: viewpoint.CameraViewpoint.Y,
          Z: viewpoint.CameraViewpoint.Z,
          Distance: viewpoint.CameraViewpoint.Distance,
          DistanceSquared: viewpoint.CameraViewpoint.DistanceSquared,
        },
        CameraDirection: {
          X: viewpoint.CameraDirection.X,
          Y: viewpoint.CameraDirection.Y,
          Z: viewpoint.CameraDirection.Z,
          Length: viewpoint.CameraDirection.Length,
          LengthSquared: viewpoint.CameraDirection.LengthSquared,
        },
        CameraUpVector: {
          X: viewpoint.CameraUpVector.X,
          Y: viewpoint.CameraUpVector.Y,
          Z: viewpoint.CameraUpVector.Z,
          Length: viewpoint.CameraUpVector.Length,
          LengthSquared: viewpoint.CameraUpVector.LengthSquared,
        },
      };

      if (viewpoint.ClippingPlanes) {
        console.log({ planes: viewpoint.ClippingPlanes });

        vp.ClippingPlanes = {
          Enabled: viewpoint.ClippingPlanes.Enabled,
        };

        const Planes = viewpoint.ClippingPlanes.Planes;
        if (Planes && Planes.length > 0) {
          vp.Planes = Planes.map((plane) => {
            return { Distance: plane.Distance, Normal: plane.Normal };
          });
          vp.Linked = viewpoint.ClippingPlanes.Linked;
        }

        const OrientedBox = viewpoint.ClippingPlanes.OrientedBox;
        if (OrientedBox) {
          vp.ClippingPlanes.OrientedBox = {
            Type: OrientedBox.Type,
            Box: { 0: OrientedBox.Box[0], 1: OrientedBox.Box[1] },
            Rotation: OrientedBox.Rotation,
          };
        }
      }

      if (viewpoint.$type === 'PerspectiveCamera') {
        view.viewpoint = {
          ...vp,
          ...{
            FieldOfView: viewpoint.FieldOfView,
            AspectRatio: viewpoint.AspectRatio,
          },
        };
      }
      if (viewpoint.$type === 'OrthogonalCamera') {
        view.viewpoint = {
          ...vp,
          ...{
            ViewToWorldScale: viewpoint.ViewToWorldScale,
          },
        };
      }
    }
    view = { ...view, created: now, issueId: issueId, deleted: false };

    commit('SET_SELECTED_ISSUE', issueId);

    db.collection('views').add(view);
  },
  ADD_ISSUE: async (_, { number = 1, created, workshopId, deleted }) => {
    const issue = {
      created,
      number,
      workshopId,
      deleted,
    };

    return db
      .collection('issues')
      .add(issue)
      .then((i) => {
        issue.id = i.id;
        return issue;
      });
  },
  ISSUE_VIEW: ({ commit }, { issue, workshop }) => {
    commit('SET_SELECTED_ISSUE', issue);
    const now = firebase.firestore.Timestamp.fromDate(new Date());

    console.log({ issue });
    db.collection('workshops').doc(workshop).update({
      viewingIssue: issue,
      lastViewed: now,
    });
  },
  DELETE_IISUE: ({ commit }, issueId) => {
    commit('SET_SELECTED_ISSUE', '');

    // delete the issue
    db.collection('issues').doc(issueId).update({ deleted: true });

    // * delete all the views associated with the issue.
    // * this reduces the size of any views queries only
    // * for those not associated to deleted issues.

    db.collection('views')
      .where('issueId', '==', issueId)
      .get()
      .then((views) => {
        if (!views.empty) {
          views.docs.forEach((document) => {
            document.ref.update({ deleted: true });
          });
        }
      });
    // ? Need to have a think about needing to renumber the remaining list.
    // ? todo: It may instead be easier to re-use an issue rather than delete it.
    // ? todo: And the ability to renumber them in the interface if need be.
  },
  TOGGLE_VIEW_DELETED: (_, { viewId, deleted }) => {
    if ((viewId && deleted === true) || deleted === false) {
      return db.collection('views').doc(viewId).update({ deleted });
    }
  },
  UPDATE_ISSUE_FIELD: (_, { issueId, field, value }) => {
    let ref;
    if (issueId && field) {
      ref = db.collection('issues').doc(issueId);
      return ref.update({ [field]: value });
    }
    return;
  },
  RECORD_ISSUE_COMMIT: (
    { commit },
    { issueId, commit: commitId, stream, object }
  ) => {
    commit('SET_ISSUE_COMMIT', { issueId, commit, stream, object });

    db.collection('issues')
      .doc(issueId)
      .update({
        speckle_commit: commitId,
        speckle_stream: stream,
        speckle_commit_object: object,
      });
  },
};
