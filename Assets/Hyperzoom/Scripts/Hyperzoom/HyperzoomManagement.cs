using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using Fungus;

public class HyperzoomManagement : MonoBehaviour
{

    #region Public Properties

    [Header("X-Ray")]

    /// <summary>
    /// The background color used for x-ray mode
    /// </summary>
    [Tooltip("The background color used for x-ray mode")]
    [SerializeField]
    private Color xrayBackground = Color.gray;

    [Header("References")]

    /// <summary>
    /// The Input Prefab (with EventSystem)
    /// </summary>
    //[Tooltip("The Input Prefab (with EventSystem)")]
    //[SerializeField]
    [HideInInspector]
    private GameObject inputPrefab;

    #endregion


    #region Properties

    /// <summary>
    /// Reference access to the FungusSceneManager
    /// </summary>
    protected FungusSceneManager fungusSceneManager = null;

    protected Camera currentCamera = null;
    protected Color backgroundColor = Color.gray;

    /// <summary>
    /// The list of all targets that can be renderered "Focusable"
    /// </summary>
    protected List<GameObject> zoomableTargets = new List<GameObject>();

    /// <summary>
    /// The list of focusable renderers, associated with its root parent GameObject
    /// </summary>
    protected Dictionary<GameObject, GameObject> zoomableFaders = new Dictionary<GameObject, GameObject>();

    /// <summary>
    /// The list of focusable renderers, associated with its root parent GameObject
    /// </summary>
    protected Dictionary<GameObject, GameObject> unzoomableFaders = new Dictionary<GameObject, GameObject>();

    #endregion


    #region Init

    virtual protected void Awake()
    {
        EnableEventSystem();
        //EnablePhylactery();
    }


    virtual protected void Start()
    {
        // memorize Renderers (for fading)
        MemorizeFaders();

        // check to see if there is a Manager
        fungusSceneManager = GameObject.FindObjectOfType<FungusSceneManager>();

        // get a reference to this scene's camera
        foreach (Camera camera in Camera.allCameras)
        {
            if (camera.scene != this.gameObject.scene)
            {
                currentCamera = camera;
            }
        }
        // if this isn't a camera
        if (currentCamera == null)
        {
            // last ditch fallback
            Debug.LogWarning("defaulting to main camera");
            currentCamera = Camera.main;
        }

        // memorize the background color
        backgroundColor = currentCamera.backgroundColor;
        SendBackgroundColor(backgroundColor);

        // if the manager is present
        if (fungusSceneManager != null)
        {
            // clean up everything in that case
            DisableAudioListeners();
            DisableCameraTransparency();
        }
        else // there is no Manager
        {
            // what to activate/deactivate when there is no manager
        }
    }

    #endregion


    #region Activations

    protected void EnableEventSystem()
    {
        // get all the listeners
        EventSystem[] eventSystems = GameObject.FindObjectsOfType<EventSystem>();
        //EventSystem[] eventSystems = GetComponents<EventSystem>();

        // if there are no EventSystems
        if (eventSystems.Length == 0)
        {
            // make sure we have a Prefab ready
            if (inputPrefab == null)
            {
                Debug.LogError("Missing Input Prefab (to Load EventSystem)");
                return;
            }

            // ok, there are no Inputs with EventSystems (this is probably a Scene-specific editing session)

            // Instantiate the Input system, with a name
            GameObject go = Instantiate(inputPrefab) as GameObject;
            go.name = "Input EventSystem";
            // make it a child of this GameObject
            go.transform.parent = this.transform;
        }

    } // EnableEventSystems()

    #endregion


    #region Deactivations

    /// <summary>
    /// Turn off all the listeners in other scenes
    /// Determines if we need to activate/deactivate various aspects of loaded scenes
    /// </summary>

    protected void DisableAudioListeners()
    {
        // get all the listeners
        AudioListener[] listeners = GameObject.FindObjectsOfType<AudioListener>();

        // go through each listener
        foreach (AudioListener listener in listeners)
        {
            // if this scene is not in our scene
            if (listener.gameObject.scene != this.gameObject.scene)
            {
                // turn off the listener
                listener.enabled = false;
            }

        } // foreach(AudioListener

    } // DisableAudioListeners()


    public void DisableCameraTransparency()
    {
        // disable camera clear flags
        CameraClearFlags clearFlags = CameraClearFlags.Nothing;

        // get all the listeners
        Camera[] cameras = GameObject.FindObjectsOfType<Camera>();

        // go through each Camera
        foreach (Camera camera in cameras)
        {
            // if this camera is in our scene
            if (camera.gameObject.scene == this.gameObject.scene)
            {
                //camera.gameObject.SetActive(false);
                camera.clearFlags = clearFlags;
            }
        } // foreach(Camera

    } // DisableCameraTransparency

    #endregion


    #region Backgroud

    /// <summary>
    /// Send out an event that the background color changed
    /// </summary>
    /// <param name="color">Color.</param>

    protected void SendBackgroundColor(Color color)
    {
        if (fungusSceneManager != null)
        {
            fungusSceneManager.BackgroundColorChanged(color);
        }
    } // SendBackgroundColor


    /// <summary>
    /// Mix the background color to/from the xray background color
    /// </summary>
    /// <param name="time">The current mix.</param>

    protected void SetBackgroundXrayColor(float time)
    {
        // create a temporary color
        Color newColor = Color.Lerp(backgroundColor, xrayBackground, time);
        // apply new color
        currentCamera.backgroundColor = newColor;
        // send out "background color changed" event
        SendBackgroundColor(newColor);
    }

    #endregion


    #region Memorize

    protected void MemorizeTargets()
    {
        // clear the previous list (if for some reason it existed. Hmmm...)
        zoomableTargets.Clear();

        // first start with the root GameObjects in this scene only
        GameObject[] rootGameObjects = this.gameObject.scene.GetRootGameObjects();
        // go through each root GameObject
        foreach(GameObject rootGameObject in rootGameObjects)
        {
            // find all the Zoomable componenets, including inactive gameobjects
            Zoomable[] zoomableScripts = rootGameObject.GetComponentsInChildren<Zoomable>(true);
            // go through each zoomable script
            foreach (Zoomable zoomableScript in zoomableScripts)
            {
                // add it to our list
                zoomableTargets.Add(zoomableScript.gameObject);
            } 
        }
    } // MemorizeTargets()


    protected void MemorizeFaders()
    {
        // first start with the root GameObjects in this scene only
        GameObject[] rootGameObjects = this.gameObject.scene.GetRootGameObjects();

        // go through each root GameObject
        foreach(GameObject rootGameObject in rootGameObjects)
        {
            // now find all the zoomable objects, including inactive GameObjects
            Zoomable[] zoomableScripts = rootGameObject.GetComponentsInChildren<Zoomable>(true);
            // go through each zoomable script
            foreach (Zoomable zoomableScript in zoomableScripts)
            {
                // get all the children inside this zoomable script
                Renderer[] childRenderersOfFocusableGameObject = zoomableScript.gameObject.GetComponentsInChildren<Renderer>(true);
                // go through all it's children
                foreach (Renderer childRenderer in childRenderersOfFocusableGameObject)
                {
                    // memorize all the children under this renderer that has a renderer
                    MemorizeChildFaders(zoomableScript.gameObject);
                }

                // TODO: Add Skinned Mesh Renderers to list
                SkinnedMeshRenderer[] childSkinMeshRenderersOfFocusableGameObject = zoomableScript.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                // go through all it's children
                foreach (SkinnedMeshRenderer childRenderer in childSkinMeshRenderersOfFocusableGameObject)
                {
                    // memorize all the children under this renderer that has a renderer
                    MemorizeChildFaders(zoomableScript.gameObject);
                }
            }
            // foreach(Zoomable
        }
        // foreach(GameObject

        // used for testing key-value pairs
        GameObject parentObjectTester;

        // go through each root GameObject
        foreach (GameObject rootGameObject in rootGameObjects)
        {
            // go through all the renderers in this scene
            Renderer[] possibleRenderers = rootGameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer possibleRenderer in possibleRenderers)
            {
                // if this is not a focusable object (and therefore not already in the list)
                if (possibleRenderer.gameObject.GetComponent<Zoomable>() == null)
                {
                    GameObject possibleRendererGameObject = possibleRenderer.gameObject;
                    // make sure it isn't already added
                    if (zoomableFaders.TryGetValue(possibleRendererGameObject, out parentObjectTester)) continue;
                    if (unzoomableFaders.TryGetValue(possibleRendererGameObject, out parentObjectTester)) continue;
                    // ok, add it to the list of unfocuseable objects
                    unzoomableFaders.Add(possibleRendererGameObject, possibleRenderer.gameObject);
                }
                // if (possibleRenderer.gameObject
            }
            // foreach(Renderer
        }
        // foreach(GameObject

        // go through each root GameObject
        foreach (GameObject rootGameObject in rootGameObjects)
        {
            // get an array of all SkinnedMeshRenderers in this scene
            SkinnedMeshRenderer[] possibleSkinnedMeshRenderers = rootGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            // go through each potential SkinnedMeshRenderer
            foreach (SkinnedMeshRenderer possibleRenderer in possibleSkinnedMeshRenderers)
            {
                // if this is not a focusable object (and therefore not already in the list)
                if (possibleRenderer.gameObject.GetComponent<Zoomable>() == null)
                {
                    GameObject possibleRendererGameObject = possibleRenderer.gameObject;
                    // make sure it isn't already added
                    if (zoomableFaders.TryGetValue(possibleRendererGameObject, out parentObjectTester)) continue;
                    if (unzoomableFaders.TryGetValue(possibleRendererGameObject, out parentObjectTester)) continue;
                    // ok, add it to the list of unfocuseable objects
                    unzoomableFaders.Add(possibleRendererGameObject, possibleRenderer.gameObject);

                }
                // if (possibleRenderer
            }
            // foreach(SkinnedMeshRenderer
        }
        // foreach(GameObject
    }
    // MemorizeFaders()


    void MemorizeChildFaders(GameObject rootParentObject)
    {
        // we want to add all the children of this rootObject that contain Renderers
        Renderer[] childRenderers = rootParentObject.GetComponentsInChildren<Renderer>(true);
        // go through all it's children
        foreach (Renderer childRenderer in childRenderers)
        {
            // check to see if this child is already in the list
            MemorizationCheckChild(rootParentObject, childRenderer.gameObject);

        } // foreach (Renderer

    } // MemorizeChildFaders()


    void MemorizationCheckChild(GameObject rootParentObject, GameObject childGameObject)
    {
        // used for testing key-value pairs
        GameObject rootParentObjectTester;
        // if it's in the unfocused list
        if (unzoomableFaders.TryGetValue(childGameObject, out rootParentObjectTester))
        {
            // remove it from this list
            unzoomableFaders.Remove(childGameObject);
        }

        // make sure it isn't already added to the focused list
        if (!zoomableFaders.TryGetValue(childGameObject, out rootParentObjectTester))
        {
            // add it to the dictionary, along with its root parent GameObject
            zoomableFaders.Add(childGameObject, rootParentObject);
        }
    }

    #endregion

} // FocusManagement
