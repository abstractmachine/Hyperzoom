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
3. Attach this `SceneManager` window to the Unity interface
4. Click `Create 'SceneManager'` button. Point the file explorer to the `Scenes` folder created in step #1. This will create a `SceneManager.scene` in this folder
	- The `Scene Manager` window should now display the list of managed scenes (currently empty)
	- The `Scene Manager` window should also display buttons for adding new scenes to the project
5. The `Scene Management` flowchart has automatically created a `Scene Request` command on Game Start. Without this command, no scene would be loaded at the beginning of the game. By default this scene is named 'Start'. You will have to create this opening scene
	- If you wish to verify this command: in the `Hierarchy` window, open the `Flowcharts` Game Object triangle and select `SceneManagement`
	- If the flowchart is not already attached to your interface: inside the `SceneManagement`'s `Inspector` window, push the `Open Flowchart Window` button. This will open the `SceneManagement` flowchart. Drag this `Flowchart` window's tab to attach it to the Unity interface
	- Select the `<Game Started>` block with the label `Start`
	- You should now see the `Request Scene' command
6. Activate the `Add Hyperzoom` option
	- You should see a toggle button marked `Add Hyperzoom`. It should be right below `Create New Scene`
	- If you do not see an `Add Hyperzoom` option, you have not installed `Hyperzoom` & `Fungus Scene Manager` correctly. Cf. `Install` instructions
7. Create a new scene named `Start` via the `Create New Scene` button. You can use the same folder from step #1
8. In the `Scene Manager` window, under `Current Scenes`, press the `CLOSE` button next to the scene named `Start`
9. Press the `Play` button in Unity and verify that the `SceneManager` successfully loads the `Start` scene

Zoomable Objects
======
These instructions describe how to create a new Zoomable Object in your project.

1. If it is not already loaded, press the `Load 'SceneManager'` button in the `Scene Manager` window
	- If it is not already open, you will find the `Scene Manager` window via the menu item `Tools > Fungus Manager > Scene Manager Window`
2. Open the `Start` scene via the `LOAD` button in the `SceneManager` window
3. In this `Start` scene, add a 3D cube in the `Hierarchy` window via `Create > 3D Object > Cube`
	- If the `Cube` was accidentally created in the `SceneManager` scene, move it down into the `Start` scene
	- By default, the `Scene Manager` is the 'Active Scene' in Unity. If you wish to quickly create new objects in the `Start` scene, right-click or ctrl-click on the scene name and choose `Game Object > ...`
4. You will have to add a fader material to the Cube
	- **All** objects must have an alpha-enabled (transparent) shader/material for Hyperzoom to work
	- The transparency of this material must be controlled via the alpha channel of the `Main Color` parameter
	- No other shader parameters are currently affected, but this can easily be changed in the future (hint: feature request)
5. Create a new fader material in your project : `Tools > Hyperzoom > Create Fader Material`. Select a folder in your own `Project` folder (i.e. avoid the `Fungus`, `Cinemachine`, `FungusManager`, `Hyperzoom` folders)
	- This will import both a `Basic-Fader.shader` and `Basic-Fader.mat` into your project
6. Apply this material to your object either by dragging the `Basic-Fader` material onto `Cube` in the `Hierarchy` window or by dragging it directly onto the 3D object in the `Scene` window
7. Add a `Zoomable` script:
	- Select the `Cube` game object in the `Hierarchy` window
	- In the `Cube`'s Inspector, select `Add Component > Scripts > Zoomable`
8. Add a scene change command when zooming into this game object:
	- In the `Hierarchy` window of the `SceneManager` scene, open the `Flowcharts` triangle next to the game object and select `SceneManagement`
	- Inside the `SceneManagement`'s `Inspector` window, push the `Open Flowchart Window` button
	- If necessary, drag this `Flowchart` window's tab to attach it to the Unity interface
	- Create a new block in this `SceneManagement` flowchart
	- Name the block `Start : Cube >`
	- Select this block in the flowchart
	- In the Inspector, select the `Execute On Event` drop-down menu and choose `SceneManager > Zoomed In`. Two text fields should appear:
		- In the `Scene Name` field, enter the text `Start`
		- In the `Object Name` field, enter the name `Cube` exactly the same as the name of the object created in step #3
	- In the `Commands` list below, press the `+` button to add a new command: `SceneManager > Request Scene`
	- Select this command. Under `Scene Name` enter `Demo`
	- This will request a scene named `Demo` whenever we zoom into a `Zoomable` `Cube` object. You will have to create this `Demo` scene
9. Save the `Start` and `SceneManager` scenes
10. Close the `Start` scene via the `Scene Manager` window's `CLOSE` button
11. Create a new scene in the `Scene Manager` window. Name this scene `Demo`
	- This is the scene that your game will switch to. Try changing the background color to make it more obvious that you have changed scenes. Also note the scene names in the `Hierarchy` window
12. Press play to test this scene transition by zooming into the object `Cube`

Fungus
======
This is a fork of the Fungus project (cf. https://github.com/fungusgames/fungus/). This code will not do anything on its own. It must be integrated into the FungusManager (cf. https://github.com/abstractmachine/FungusManager)


Author
======
This project was created by Douglas Edric Stanley (http://www.abstractmachine.net) for the Atelier Hypermédia of the Aix-en-Provence School of Art (cf. http://www.ecole-art-aix.fr) and for the Media Design Master of The Geneva University of Art & Design, –HEAD Genève (cf. http://head.hesge.ch). It is notably used in the project A Crossing Industry, a collaboration with Cédric Parizot, IREMAM (CNRS-AMU)/IMÉRA (cf. https://github.com/abstractmachine/ACrossingIndustry)

License
=======
This code is not yet licensed for use outside of the project `A Crossing Industry`.

