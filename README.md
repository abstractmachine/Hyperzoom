Hyperzoom
==================
WTF
======
This is a snow-globe-style rotate & pinch interface for peeking into 3D hyperspace. It allows you to jump from one scene to the next by zooming into or out of `Zoomable` 3D objects. In this sense, it acts like a hyperlink system similar to the World Wide Web, only in this case the links are through 3D objects and take place in a 3D environment.

Install
======
1. Create a new (3D) Unity project
2. Download the [latest FungusManager release](https://github.com/abstractmachine/FungusManager/releases/latest)
	- This package includes its own Fungus folder, compatible with this release
	- If necessary, you should be able to replace this Fungus folder with a more recent update
3. Open the FungusManager package to import all the files into your Unity project
4. Download the [latest Hyperzoom release](https://github.com/abstractmachine/Hyperzoom/releases/latest)
5. Open the Hyperzoom package to import all the files into your Unity project.

New Project
======
These instructions describe how to create a basic project managed by the `Scene Manager`.

1. Create a new folder named `Scenes` in your project
2. Open SceneManager window via menu item `Tools > FungusManager > Scene Manager Window`
3. Attach SceneManager to the Unity interface
4. Click `Create 'SceneManager'` button. Point the file explorer to the `Scenes` folder created in step #1. This will create a `SceneManager.scene` in this folder
5. Verify that the `Add Hyperzoom` button exists in the `SceneManager` window:
	- You should see a toggle button marked `Add Hyperzoom`. It should be right below `Create New Scene`
	- If you do not see a `Add Hyperzoom` button, you have not installed Hyperzoom & Fungus Scene Manager correctly
6. Activate the `Add Hyperzoom` button
7. Create a new scene named `Start` via the `Create New Scene` button. You can use the same folder from step #1
8. In the `Hierarchy` window, open the `Flowcharts` Game Object triangle and select `SceneManagement`
9. Inside the `SceneManagement`'s `Inspector` window, push the `Open Flowchart Window` button
10. Drag this `Flowchart` window's tab to attach it to the Unity interface
11. Select the `<Game Started>` block with the label `Start`
12. Inside the Inspector, press the `+` button to add a new command `SceneManager > Request Scene`
13. Select the new `Request Scene` command
14. Under the `Scene Name` field, enter the name `Start`. Avoid adding extraneous invisible spaces in this name
15. In the `Scene Manager` window, press the `CLOSE` button next to the scene name `Start` under `Current Scenes`
16. Press the `Play` button in Unity and verify that the `SceneManager` successfully loads the `Start` scene

Zoomable Objects
======
These instructions describe how to create a new Zoomable Object in your project.

1. If it is not already loaded, press the `Load 'SceneManager'` button in the `Scene Manager` window
2. Open the `Start` scene via the `LOAD` button in the `SceneManager` window
3. In the `Start` scene, add a 3D cube in the `Hierarchy` window via `Create > 3D Object > Cube`
	- If the `Cube` is created in the `SceneManager` scene, move it down into the `Start` scene
4. Rename the 3D cube to `TestCube`
5. Create a new fader material : `Tools > Hyperzoom > Create Fader Material`
6. Apply this material either by dragging the `Basic-Fader` material onto `TestCube` in the `Hierarchy` window or by dragging it directly onto the object in the `Scene` window
7. Add a `Zoomable` script:
	- Select the `Cube` game object in the `Hierarchy` window
	- In the `Cube` Inspector, select `Add Component > Scripts > Zoomable`
8. Add a scene change command when zooming into this game object:
	- In the `Hierarchy` window of the `SceneManager` scene, open the `Flowcharts` Game Object triangle and select `SceneManagement`
	- Inside the `SceneManagement`'s `Inspector` window, push the `Open Flowchart Window` button
	- If necessary, drag this `Flowchart` window's tab to attach it to the Unity interface
	- Create a new block in this `SceneManagement` flowchart
	- Name the block `Start : TestCube >`
	- Select this block in the flowchart
	- In the Inspector, select the `Execute On Event` drop-down menu and choose `SceneManager > Zoomed In`. Two text fields should appear:
	- In the `Scene Name` field, enter the text `Start`
	- In the `Object Name` field, enter the text `TestCube` exactly as in step #4
	- In the `Commands` list below, press the `+` button to add a new command `SceneManager > Request Scene`
	- Select this command. Under `Scene Name` enter `Demo`
9. Save the `Start` scene
10. Close the `Start` scene via the `Scene Manager` window
11. Create a new scene in the `Scene Manager` window. Name this scene `Demo`
	- This is the scene that your game will switch to. Try changing the background color to make it more obvious that you have changed scenes. Also note the scene names in the `Hierarchy` window
12. Press play to test this scene transition by zooming into the object `TestCube`

Fungus
======
This is a fork of the Fungus project (cf. https://github.com/fungusgames/fungus/). This code will not do anything on its own. It must be integrated into the FungusManager (cf. https://github.com/abstractmachine/FungusManager)


Author
======
This project was created by Douglas Edric Stanley (http://www.abstractmachine.net) for the Atelier Hypermédia of the Aix-en-Provence School of Art (cf. http://www.ecole-art-aix.fr) and for the Media Design Master of The Geneva University of Art & Design, –HEAD Genève (cf. http://head.hesge.ch). It is notably used in the project A Crossing Industry, a collaboration with Cédric Parizot, IREMAM (CNRS-AMU)/IMÉRA (cf. https://github.com/abstractmachine/ACrossingIndustry)

License
=======
This code is not yet licensed for use outside of the project `A Crossing Industry`.

