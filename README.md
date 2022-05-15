# Rimshot

<img src="https://github.com/jsdbroughton/rimshot/blob/main/logo.png?raw=true" alt="logo" width="200"/>

## [R]ealtime [I]ssue [M]anagement with [S]peckle - HOT! 🔥

[https://rimshot.app/](https://rimshot.app/issues/vWomnz77p44ZW4Hi350B)

## Premise ##

During the Great Lockdown of the last two years, in what has so quickly become a remote-working, online-first-meetings, broadcast only way of doing business. One really striking aspect of the AECO world, away from the Teams, Google Docs, Zoom and the like, was that the manner of Model Analysis and review has not been in the same class of technological delivery.

I've sat through many dozens of streamed Navisworks sessions and engagement amongst teams has definitely waned or defaulted to a broadcast rather than a collaboration. Attention spent managing the remote experience means the analyst is taxed to the point of only making scant notes about issues, and contributors may not all be heard to register their input.

For this professional/social need I introduce Rimshot - Realtime Issue Management with Speckle.

Utilising two aspects that I admire of Speckle: 

1. Information Hub and low-friction data transfer.
2. Bring people to the Model. Speckle is multiplayer.

[<img src="https://img.youtube.com/vi/1lUDgHJP7H4/maxresdefault.jpg" alt="logo" width="300"/><br />Watch the submission video](https://www.youtube.com/watch?v=1lUDgHJP7H4)

## Usage ##
### Adding details to issues 
1. Install the addin as a application plugin bundle
2. Launch Navisworks and select plugin<br />![Screenshot 2022-05-15 at 15 52 06](https://user-images.githubusercontent.com/760691/168479138-d8a8c4bf-23dc-4448-af03-82c6c97bdbaf.png)

3. Select a project and workshop (already created in the rimshot database)<br />![Screen Recording 2022-05-15 at 15 29 55 2022-05-15 15_31_59](https://user-images.githubusercontent.com/760691/168478184-d156c87e-fc48-4bca-976e-978b32da90f5.gif)

4. Select an issue to commit to
5. Elements selected in Navisworks will be committed as Speckle Mesh to the corresponding branch (created new if not existing)<br />![Screen Recording 2022-05-15 at 15 26 00 2022-05-15 15_28_19](https://user-images.githubusercontent.com/760691/168478021-95c36dff-27c2-4c62-8848-6a124c08b017.gif)

6. Selecting **Snapshot** adds an image to the existing issue of whatever is visible in the viewport. If no issue is selected, snapshotting ceates a new issue and corresponding branch.<br />![Screen Recording 2022-05-15 at 15 12 20 2022-05-15 15_14_31](https://user-images.githubusercontent.com/760691/168477299-89156cdb-2b73-4546-9329-597f97756711.gif)


### Contributors
1. The analyst can share a unique secure URL to the workshop record. (https://rimshot.app/issues/{UNIQUE ID})<br />![Screen Recording 2022-05-15 at 15 33 22 2022-05-15 15_35_01](https://user-images.githubusercontent.com/760691/168478466-3fd4b879-8d4b-4619-88f6-59c27ecfb1e7.gif)
2. All visitors to that web page can see images and Speckle commits being added. <br />![Screen Recording 2022-05-15 at 15 35 29 2022-05-15 15_37_40](https://user-images.githubusercontent.com/760691/168478471-2aa752d1-6eef-4056-9b26-16b709819fb3.gif)

3. "Follow-mode" scrolls to the issue currently being presented by the Analyst. Selecting a different issue cancels Follow Mode<br />![image](https://user-images.githubusercontent.com/760691/168476968-7060e291-9d7e-498a-b3b8-f8ed4938c3e7.png)

4. All contributors can comment and edit the descriptive and the metadata fields.<br />![image](https://user-images.githubusercontent.com/760691/168476993-6ccfe39e-8123-417c-9f40-004b8a831344.png)
5. The commited model can be viewed in the Rimshot app or the corresponding Speckle Server branch.<br />![Screen Recording 2022-05-15 at 15 54 31 2022-05-15 15_56_51](https://user-images.githubusercontent.com/760691/168479389-8659d7ae-62b0-4dd3-a7ba-a69a2e0c57f8.gif)


6. The workshop record can be downloaded as in CSV file format.<br />![image](https://user-images.githubusercontent.com/760691/168476944-d886be18-3d9e-46f0-ab43-dac61ba2c3e0.png)



## Components ##

The hackathon submission consists of:

### An installed webview addin for Navisworks.
<img src="https://user-images.githubusercontent.com/760691/168424755-e6e887e3-89f0-42aa-9f43-ee712722ace1.png" alt="logo" width="300"/>
* Model elements selected in Navis will be committed and synced to the app. 
* Model elements are converted on the fly while the discussion continues.
* The addin allows for immediate viewport screen capture.

### An Issue Mangement workshop capture interface.
<img src="https://user-images.githubusercontent.com/760691/168424792-bcc5870c-e63c-4996-9a8c-4b997917f0b4.png" alt="logo" width="300"/>
* Structuring the database around a collaborative experience aids quality of Issue resolution. 

### A realtime multi-party data capture app and database for desktop and mobile devices.
<img src="https://user-images.githubusercontent.com/760691/168424849-7aca1839-bf19-4226-a1e6-f6d651f1158f.png" alt="logo" width="300"/> <img src="https://user-images.githubusercontent.com/760691/168424963-9d311426-499f-4997-bd9a-5f119f70e821.gif" alt="logo" width="300"/>
* Using a real-time database allows for all authenticate users to comment and properly describe and issue and its path to resolution.* The responsive app design scales to full screen to handheld.
* Contributions can be immediate and collaborative.
* Backend webhook and cloud function support to reinforce the Navis<>App interaction and capture events that may fail due to connecttivity issues or lag. Both events may happen without conflict and eventual consistency is assured. 

### A pattern for managing Model based issues within the Speckle interface.
<img src="https://user-images.githubusercontent.com/760691/168424905-7967c8c1-0065-44dd-8142-eefc0a5a453d.png" alt="logo" width="300"/>
* Each issue is a separate Speckle branch. 
* Multiple sub-issues can be made as separate commits to the branch.
* New issue branches are created automatically

## Roadmap Post Hackathon ##

* Proper linking with Auth flow and project/stream selection.
* Syncing of Comments between the two databases - near realtime.
* More responsive syncing. Allow Speckle to push to the app (perhaps closing out issues, 3rd party commits)
* Sorting out the 🔥HOT MESS🔥 of the Navisworks geometry translation.
<img src="https://user-images.githubusercontent.com/760691/168424674-539b5dd1-db83-4bdf-98aa-b4cfd2bc1d0e.png" alt="logo" width="500"/>

* Integate the Speckle Viewer rather than embed views.
* Responsive Event driven model analysis to push metadata into the issue record.
