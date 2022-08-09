export const dummySenders = [
  {
    colorMode: false,
    coordinateSystem: 'model',
    hiddenMode: false,
    selectionMode: true,
    selections: [{ Guid: '1', Name: 'Selection 1' }],
    target: {
      account: {
        host: 'speckle.xyz',
        email: 'jonathon@stardotbmp.com',
      },
      branch: { name: 'main' },
      stream: { id: '216ryuwfe', name: 'Wohnsilo' },
    },
  },
  {
    colorMode: false,
    coordinateSystem: 'vector',
    hiddenMode: false,
    selectionMode: true,
    selections: [{ Guid: '1', Name: 'Selection 1' }],
    target: {
      account: {
        host: 'speckle.xyz',
        email: 'jonathon@stardotbmp.com',
      },
      stream: { id: '216ryuwfe', name: 'Wohnsilo' },
      branch: { name: 'Option im 3. Stock' },
    },
    vector: { x: 533190, y: 181700, z: 0 },
  },
  {
    colorMode: false,
    coordinateSystem: 'vector',
    hiddenMode: false,
    selectionMode: true,
    selections: [{ Guid: '1', Name: 'Selection 1' }],
    target: {
      account: {
        host: 'speckle.xyz',
        email: 'jonathon@stardotbmp.com',
      },
      stream: { id: '216ryuwfe', name: 'Test' },
      branch: { name: 'main' },
    },
    vector: { x: 533190, y: 181700, z: 0 },
  },
];
export const dummyAccounts = [
  {
    email: 'jonathon@stardotbmp.com',
    host: 'speckle.xyz',
    displayname: '[speckle.xyz] - jonathon@stardotbmp.com',
  },
  {
    email: 'rimshot@rimshot.app',
    host: 'speckle.xyz',
    displayname: '[speckle.xyz] - rimshot@rimshot.app',
  },
  {
    email: 'jonathon+test@stardotbmp.com',
    host: 'latest.speckle.dev',
    displayname: '[latest.speckle.dev] - jonathon@stardotbmp.com',
  },
];
export const dummyStreams = [
  { name: 'Tennis', id: '06c5265db1' },
  { name: 'Palettes', id: '35ad6a0c9c' },
];
export const dummyBranches = [{ name: 'main' }, { name: 'hog house' }];

export const dummySelectionSets = [
  {
    Children: [
      {
        Children: null,
        Guid: 'b61c3c0e-331d-416b-a503-e270387523fc',
        Name: '8th Floor',
        Type: 'selection',
      },
      {
        Children: null,
        Guid: '57ea0e32-4911-47f8-8755-2dabf454b367',
        Name: 'Geometry Nodes',
        Type: 'search',
      },
      {
        Children: null,
        Guid: '533f87e1-fe3f-4654-82fc-e37c413f334b',
        Name: 'railing',
        Type: 'selection',
      },
      {
        Children: null,
        Guid: '7c29a1b4-6778-4331-a992-8a33f4328af4',
        Name: 'Selection Set',
        Type: 'selection',
      },
    ],
    Guid: '618cecd7-af8a-44e4-954b-4341572a66bd',
    Name: 'Architecture',
    Type: 'folder',
  },
  {
    Name: 'MEP',
    Type: 'folder',
    Guid: 'b61c3c1e-331d-416b-a503-e270387523fc',
    Children: [
      {
        Children: null,
        Guid: '57ea1e32-4911-47f8-8755-2dabf454b367',
        Name: 'Chillers',
        Type: 'search',
      },
      {
        Children: null,
        Guid: '533187e1-fe3f-4654-82fc-e37c413f334b',
        Name: 'Plantroom',
        Type: 'selection',
      },
    ],
  },
  {
    Children: null,
    Guid: '6091e309-966c-454c-b1c9-a1238dcc070b',
    Name: 'Special Selection Set',
    Type: 'selection',
  },
];
