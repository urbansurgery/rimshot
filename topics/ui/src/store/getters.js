export default {
  liveProjects: (state) => {
    return state.projects.filter(
      (project) => project.archived != true && project.live === true
    );
  },
  singleWorkshopDetails: function (state) {
    return (id) => {
      if (!id) return;
      if (state.workshops.length === 0 || state.projects.length === 0) return;
      const selectedWorkshop = state.workshops.filter(
        (workshop) => workshop.id === id
      )[0];

      if (!selectedWorkshop) return;

      const selectedProject = state.projects.filter((project) => {
        return project?.key === selectedWorkshop?.project?.key;
      })[0];

      if (!selectedProject) return;

      return { project: selectedProject, workshop: selectedWorkshop };
    };
  },
};
