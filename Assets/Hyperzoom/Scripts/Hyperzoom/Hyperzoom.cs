using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Hyperzoom : HyperzoomManagement
{
    #region Serialized Properties 

    /// <summary>
    /// Reference to a transform when nothing is selected
    /// </summary>
    [Tooltip("Reference to a transform when nothing is selected")]
    [SerializeField]
    private GameObject unselectedTarget;

    /// <summary>
    /// Reference to our cinemachine free-look script on the virtual camera
    /// </summary>
    [Tooltip("Reference to the cinemachine free-look virtual camera")]
    [SerializeField]
    private CinemachineFreeLook freeLookCamera;

    /// <summary>
    /// Reference to the cinemachine bird's-eye view virtual camera when no object is selected
    /// </summary>
    [Tooltip("Reference to the cinemachine bird's-eye view virtual camera when no object is selected")]
    [SerializeField]
    private CinemachineFreeLook birdsEyeViewCamera;

    [Header("Zoom")]

    /// <summary>
    /// The zoomed-in value for the virtual camera
    /// </summary>
    [Tooltip("Define the zoom value of a zoomed-in camera")]
    [Range(0.0f, 100.0f)]
    [SerializeField]
    private float zoomMinimum = 2.0f;

    /// <summary>
    /// The starting value for the virtual camera
    /// </summary>
    [Tooltip("Define the starting zoom value of the virtual camera")]
    [Range(0.0f, 100.0f)]
    [SerializeField]
    private float zoomStart = 10.0f;

    /// <summary>
    /// The zoomed-out value for the virtual camera
    /// </summary>
    [Tooltip("Define the zoom value of a zoomed-out camera")]
    [Range(0.0f, 100.0f)]
    [SerializeField]
    private float zoomMaximum = 20.0f;

    /// <summary>
    /// The speed multiplier for zooming the virtual camera
    /// </summary>
    [Tooltip("Define the speed multiplier for zooming in/out the virtual camera")]
    [SerializeField]
    private float zoomSpeed = 0.5f;

    //[Header("X-Ray")]

    ///// <summary>
    ///// define the duration of x-ray mode
    ///// </summary>
    //[Tooltip("Define the duration (in seconds) of x-ray mode")]
    //[SerializeField]
    //private float xrayDuration = 2.0f;

    [Header("Fader Curves")]

    /// <summary>
    /// draw the zoom curve for the targeted GameObject (and its children)
    /// </summary>
    [Tooltip("draw the zoom curve for the targeted GameObject (and its children)")]
    public AnimationCurve targetCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(0.05f, 0.968f), new Keyframe(0.25f, 1.0f), new Keyframe(0.75f, 1.0f), new Keyframe(0.95f, 0.0f));

    /// <summary>
    /// draw the zoom curve for focusable GameObjects (and their children)
    /// </summary>
    [Tooltip("draw the zoom curve for focusable GameObjects (and their children)")]
    public AnimationCurve focusableCurve = new AnimationCurve(new Keyframe(0.0f, 0.1f), new Keyframe(0.25f, 1.0f), new Keyframe(0.75f, 1.0f), new Keyframe(0.95f, 1.0f));

    /// <summary>
    /// draw the zoom curve for unfocusable GameObjects
    /// </summary>
    [Tooltip("draw the zoom curve for unfocusable GameObjects")]
    public AnimationCurve unfocusableCurve = new AnimationCurve(new Keyframe(0.11f, 0.0f), new Keyframe(0.25f, 1.0f), new Keyframe(0.75f, 1.0f), new Keyframe(0.9f, 0.0f));

    #endregion


    #region Coroutine Properties

    /// <summary>
    /// A coroutine that stops the previous rotation events if no new events
    /// come in after one frame. Used to control Cinemachine's Free-Look system.
    /// </summary>
    private Coroutine rotationStopRoutine = null;

    /// <summary>
    /// A coroutine that temporarily turns on dampening on the Free-Look virtual camera,
    /// then fades out the dampening effect
    /// </summary>
    private Coroutine freeLookDampeningRoutine = null;

    #endregion

    #region Zoom Properties

    private bool isOrthographic = false;

    private float zoomStartingPct = 0.0f;
    private float zoomTargetPct = 0.5f;
    private float zoomFadeMargin = 0.25f;
    private float zoomPointOfNoReturn = 0.5f;
    private bool zoomIsSnapping = false;
    private bool freeLookIsDamping = false;
    float zoomPointOfNoReturnLow = 0.0f;
    float zoomPointOfNoReturnHigh = 1.0f;

    #endregion


    #region X-Ray Properties

    private bool xrayState = false;
    private float xrayOpacityValue = 1.0f;
    private float xraySpeed = 0.05f;
    //private float xrayCountdown = 0.0f;

    #endregion


    #region Targeting properties

    /// <summary>
    /// the current selected gameObject we are targeted on
    /// </summary>
    [Tooltip("Define the starting selected gameObject we are targeted on (optional - leave null for unselected-at-start)")]
    [SerializeField]
    private GameObject target;

    #endregion


    #region Events

    /// <summary>
    /// Fires when scene change has started
    /// </summary>
    public static event Action<string> ZoomInStarted;

    /// <summary>
    /// Fires when scene change has finished
    /// </summary>
    public static event Action<string> ZoomInFinished;

    /// <summary>
    /// Fires when scene change has started
    /// </summary>
    public static event Action<string> ZoomOutStarted;

    /// <summary>
    /// Fires when scene change has finished
    /// </summary>
    public static event Action<string> ZoomOutFinished;

    #endregion


    #region Init

    override protected void Start()
    {
        base.Start();

        TargetInit();

        // find out if we're orthographic from the Cinemachine brain
        CinemachineBrain brain = CinemachineCore.Instance.FindPotentialTargetBrain(freeLookCamera);
        isOrthographic = (brain != null) ? brain.OutputCamera.orthographic : false;

        // force the camera to the starting value
        if (isOrthographic) freeLookCamera.m_Lens.OrthographicSize = zoomStart;
        else freeLookCamera.m_Lens.FieldOfView = zoomStart;

        // remember this starting value
        zoomStartingPct = zoomTargetPct = ConvertZoomToPct(zoomStart);

        // adjust speed if we're an orthographic camera
        if (isOrthographic) zoomSpeed *= 0.2f;

        // figure out the points of no-return
        zoomPointOfNoReturnLow = zoomFadeMargin * zoomPointOfNoReturn;
        zoomPointOfNoReturnHigh = 1.0f - (zoomFadeMargin * zoomPointOfNoReturn);

        // fade in scene
        HideEverything();
        StartCoroutine("FadeInEverything");

    }

    #endregion


    #region Rotation

    /// <summary>
    /// This handler is called whenever there is a rotation request
    /// </summary>
    /// <returns>The rotate.</returns>
    /// <param name="dragObject">Drag object.</param>
    /// <param name="delta">Delta.</param>

    void Rotate(GameObject dragObject, Vector3 delta)
    {
        // if we're snapping, it's the end of the scene. Stop all incoming processes
        if (zoomIsSnapping) return;

        // if we're damping, turn it off (creates ugly behavior while rotating)
        if (freeLookIsDamping) AbortFreeLookDamping();

        // if there is a co-routine running, stop it
        CancelResetPerspective();

        Vector2 rotationDelta = new Vector2(delta.x, delta.y);

        float rotationMultiplier = (1000.0f / (float)Screen.width) * 0.0002f;

        rotationDelta.x *= rotationMultiplier;
        rotationDelta.y *= rotationMultiplier;

        freeLookCamera.m_XAxis.m_InputAxisValue = rotationDelta.x;
        freeLookCamera.m_YAxis.m_InputAxisValue = rotationDelta.y;

        // cancel the previous turn-off command
        if (rotationStopRoutine != null) StopCoroutine(rotationStopRoutine);
        // start a one-frame countdown to turn off this rotation
        rotationStopRoutine = StartCoroutine(TurnOffLastRotate());
    }

    /// <summary>
    /// This routine will wait for one frame before turning off the last rotate command.
    /// This allows the following rotation event to supersede the previous, and so on, until
    /// only the last rotation remains and can cancel only the last rotation value.
    /// </summary>
    /// <returns>The off last rotate.</returns>

    IEnumerator TurnOffLastRotate()
    {
        // wait for one frame
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();

        // turn off rotations
        freeLookCamera.m_XAxis.m_InputAxisValue = 0f;
        freeLookCamera.m_YAxis.m_InputAxisValue = 0f;

        // all done
        yield return null;
    }

    #endregion


    #region Zoom

    /// <summary>
    /// This handler is called whenever there is a zoom request
    /// </summary>
    /// <returns>The zoom.</returns>
    /// <param name="delta">Delta.</param>

    void Zoom(float delta)
    {
        // if we're snapping, it's the end of the scene. Stop all incoming processes
        if (zoomIsSnapping) return;

        float zoomMultiplier = (100.0f / (zoomMaximum - zoomMinimum)) * 0.1f;

        delta *= zoomMultiplier;

        // turn off any snapping to original perspective
        CancelResetPerspective();

        // slow down delta
        delta = ZoomSlowDownDelta(delta);
        // first apply this value to our target
        zoomTargetPct += delta * zoomSpeed;
        // clamp the field of view to make sure it doesn't go beyond 0.0f and 1.0f
        zoomTargetPct = Mathf.Clamp(zoomTargetPct, 0.05f, 0.95f);

        // zoom to this new value
        ZoomToPct(zoomTargetPct);

        // reset the xray countdown (if it's waiting)
        TurnOffXray();
    }


    void ZoomToPct(float zoomPctValue)
    {
        // now scale up to zoom values
        float zoomCameraValue = ConvertPctToZoom(zoomPctValue);

        // if the camera is in orthographic mode
        if (isOrthographic)
        {
            // apply to camera
            freeLookCamera.m_Lens.OrthographicSize = zoomCameraValue;
        }
        else // otherwise it's in perspective mode
        {
            // apply to camera
            freeLookCamera.m_Lens.FieldOfView = zoomCameraValue;
        }
        // apply these changes
        ApplyRendererOpacities();
    }


    void ZoomIn(bool useless = false)
    {
        // if we're snapping, it's the end of the scene. Stop all incoming processes
        if (zoomIsSnapping) return;
        // turn off any snapping to original perspective
        CancelResetPerspective();

        float newZoomTargetPct = zoomTargetPct;

        if (target != null && newZoomTargetPct < 0.4f) newZoomTargetPct = zoomPointOfNoReturnLow;
        else if (newZoomTargetPct >= 0.666f) newZoomTargetPct = 0.5f;
        else if (newZoomTargetPct > zoomFadeMargin) newZoomTargetPct = zoomFadeMargin;

        ZoomToward(newZoomTargetPct);
    }


    void ZoomOut(bool useless = false)
    {
        // if we're snapping, it's the end of the scene. Stop all incoming processes
        if (zoomIsSnapping) return;
        // turn off any snapping to original perspective
        CancelResetPerspective();

        float newZoomTargetPct = zoomTargetPct;

        if (newZoomTargetPct > 0.6f) newZoomTargetPct = zoomPointOfNoReturnHigh;
        else if (newZoomTargetPct < 0.333f) newZoomTargetPct = 0.5f;
        else if (newZoomTargetPct < (1.0f - zoomFadeMargin)) newZoomTargetPct = 1.0f - zoomFadeMargin;

        ZoomToward(newZoomTargetPct);
    }


    void ZoomToward(float newZoomValue)
    {
        StopCoroutine("ZoomSnapTo");
        StopCoroutine("ZoomTowardRoutine");
        // start moving to new position
        StartCoroutine("ZoomTowardRoutine", newZoomValue);
    }


    IEnumerator ZoomTowardRoutine(float newZoomValue)
    {
        while (Mathf.Abs(newZoomValue - zoomTargetPct) > 0.01f)
        {
            // 
            zoomTargetPct = Mathf.Lerp(zoomTargetPct, newZoomValue, 0.25f);
            // snap to that new value
            ZoomToPct(zoomTargetPct);
            // 
            yield return new WaitForEndOfFrame();
        }
        // take on new value
        zoomTargetPct = newZoomValue;
        // snap to that new value
        ZoomToPct(zoomTargetPct);
        // clean up after new zoom position
        ZoomCleanup();
        // all done
        yield return null;
    }


    float ZoomSlowDownDelta(float delta)
    {
        // if we're at the lower level and delta is shrinking
        if (zoomTargetPct < zoomFadeMargin && delta < 0)
        {
            // normalize zoom value within the zoom margin
            float factor = ((zoomFadeMargin - zoomTargetPct) / zoomFadeMargin);
            // invert normalization
            factor = 1.0f - factor;
            // logarithmicize factor
            factor *= factor;
            // apply factor to delta
            return delta * factor;
        }

        if (zoomTargetPct > 1.0f - zoomFadeMargin && delta > 0)
        {
            // normalize zoom value within the zoom margin
            float factor = (1.0f - zoomTargetPct) / zoomFadeMargin;
            // logarithmicize factor
            factor *= factor;
            // apply factor to delta
            return delta * factor;
        }

        // return results
        return delta;
    }


    /// <summary>
    /// this method is called when we've finished or paused zooming
    /// </summary>

    void ZoomCleanup(bool useless = false)
    {
        // if we're snapping, it's the end of the scene. Stop all incoming processes
        if (zoomIsSnapping) return;

        // abort previous coroutines
        // if there was a snap forward routine
        StopCoroutine("ZoomSnapTo");
        StopCoroutine("ZoomTowardRoutine");

        // this is the target value we are going to snap to
        float snapTarget = zoomTargetPct;

        // if we're zoomed in beyond low margin and something is selected
        if (zoomTargetPct < zoomFadeMargin)
        {
            // if there is no target
            if (target == null)
            {
                // back up to edge of fade
                snapTarget = zoomFadeMargin;
            }
            // if we're beyond the point of no return and the manager is present
            else if (zoomTargetPct <= zoomPointOfNoReturnLow && managerIsPresent)
            {
                snapTarget = 0.0f;
            }
            else // otherwise, we're inside the point of no return
            {
                snapTarget = zoomFadeMargin;
            }
            // snap to this new value
            StartCoroutine("ZoomSnapTo", snapTarget);

        } // if (zoomTargetPct < zoomFadeMargin)


        // if were zoomed out beyond high margin
        if (zoomTargetPct > 1.0f - zoomFadeMargin)
        {

            // if we're beyond the point of no return and the manager is present
            if (zoomTargetPct >= zoomPointOfNoReturnHigh & managerIsPresent)
            {
                // force zoom out
                snapTarget = 1.0f;
            }
            else // otherwise, we're inside the point of no return
            {
                // abort force zoom out
                snapTarget = 1.0f - zoomFadeMargin;
            }

            // snap to this new value
            StartCoroutine("ZoomSnapTo", snapTarget);

        } // if zoomTargetPct > 1.0f - zoomFadeMargin

    } // ZoomCleanup()


    IEnumerator ZoomSnapTo(float newTargetValue)
    {
        // if this is a type of snap that will end the scene
        if (newTargetValue == 0.0f)
        {
            // set a flag to tell all incoming events to stop while we snap
            zoomIsSnapping = true;
            // signal that we're starting to transition
            StartSceneChangeForward();
        }
        else if (newTargetValue == 1.0f)
        {
            // set a flag to tell all incoming events to stop while we snap
            zoomIsSnapping = true;
            // signal that we're starting to transition
            StartSceneChangeBackward();
        }

        // how fast does this snapping work?
        float zoomSnapSpeed = 0.1f;
        // how close do we need to get to the target before we jump to that value?
        float snapTolerance = 0.01f;

        // cycle through which we're waiting for zoom target to snap into place
        while (Mathf.Abs(zoomTargetPct - newTargetValue) > snapTolerance)
        {
            // calculate the new value
            zoomTargetPct = Mathf.Lerp(zoomTargetPct, newTargetValue, zoomSnapSpeed);
            // apply that value
            ZoomToPct(zoomTargetPct);
            // wait for the next frame before moving again
            yield return new WaitForFixedUpdate();
        }
        // close enough, force that new value
        zoomTargetPct = newTargetValue;
        // now jump to that target
        ZoomToPct(newTargetValue);

        // if we were snapping forward
        if (newTargetValue == 1.0f)
        {
            // signal that we're done
            FinishSceneChangeBackward();
        }
        else if (newTargetValue == 0.0f)
        {
            // signal that we're done
            FinishSceneChangeForward();
        }
        // all done
        yield return null;
    }

    #endregion


    #region Fader

    /// <summary>
    /// Extracts the material from fader. There are two possible renderers:
    /// a simple Renderer or a SkinnedMeshRenderer. This will extract from either of the two
    /// </summary>
    /// <returns>The material found inside of the fader.</returns>
    /// <param name="fadeObject">Fade object.</param>

    Material[] ExtractMaterialsFromFader(GameObject fadeObject)
    {
        // try to extract a renderer
        Renderer renderer = fadeObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.materials;
        }
        // try to extract a SkinnedMeshRenderer
        SkinnedMeshRenderer skinnedMeshRenderer = fadeObject.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer != null)
        {
            return skinnedMeshRenderer.materials;
        }
        // error
        Debug.LogError("No Renderer or SkinnedMeshRenderer");
        return null;
    }

    void HideEverything()
    {
        // go through all the zoomed objects
        foreach (KeyValuePair<GameObject, GameObject> zoomedKeyValuePair in zoomableFaders)
        {
            FadeMaterial(ExtractMaterialsFromFader(zoomedKeyValuePair.Key), 0.0f);
        }

        // go through all the unzoomed faders
        foreach (KeyValuePair<GameObject, GameObject> unzoomedKeyValuePair in unzoomableFaders)
        {
            // apply both fading values
            FadeMaterial(ExtractMaterialsFromFader(unzoomedKeyValuePair.Key), 0.0f);
        }
    }


    IEnumerator FadeInEverything()
    {
        float fadeSpeed = 0.05f;

        // fade everything in
        for (float opacity = 0.0f; opacity < 1.0f; opacity += fadeSpeed)
        {
            opacity = Mathf.Clamp01(opacity);

            // go through all the zoomed objects
            foreach (KeyValuePair<GameObject, GameObject> zoomedKeyValuePair in zoomableFaders)
            {
                FadeMaterial(ExtractMaterialsFromFader(zoomedKeyValuePair.Key), opacity);
            }

            // go through all the unzoomed faders
            foreach (KeyValuePair<GameObject, GameObject> unzoomedKeyValuePair in unzoomableFaders)
            {
                // apply both fading values
                FadeMaterial(ExtractMaterialsFromFader(unzoomedKeyValuePair.Key), opacity);
            }

            yield return new WaitForEndOfFrame();
        }

    }


    void ApplyRendererOpacities()
    {
        float targetValue = targetCurve.Evaluate(zoomTargetPct);
        float focusedValue = focusableCurve.Evaluate(zoomTargetPct);
        float unfocusedValue = unfocusableCurve.Evaluate(zoomTargetPct) * xrayOpacityValue;
        float opaqueValue = 1.0f * xrayOpacityValue;

        // go through all the focused objects
        foreach (KeyValuePair<GameObject, GameObject> focusedKeyValuePair in zoomableFaders)
        {
            GameObject childObject = focusedKeyValuePair.Key;
            Material[] childMaterials = ExtractMaterialsFromFader(childObject);
            GameObject parentObject = focusedKeyValuePair.Value;

            // if there is no target && the xray is not on && we're at the zoom-in point
            if (target == null && !xrayState && zoomTargetPct < zoomFadeMargin)
            {
                FadeMaterial(childMaterials, opaqueValue);
            }
            // if this is the focused object, and we're zooming IN
            else if (parentObject == target && zoomTargetPct < zoomFadeMargin)
            {
                //FadeMaterial(renderer.material, 1.0f);
                FadeMaterial(childMaterials, targetValue);
            }
            // this is not the focused object but we're zooming in
            else if (parentObject != target && zoomTargetPct < zoomFadeMargin)
            {
                FadeMaterial(childMaterials, unfocusedValue);
            }
            // otherwise this is either not the focused object or we're not zooming in
            else
            {
                //FadeMaterial(renderer.material, focusedOpacity);
                FadeMaterial(childMaterials, focusedValue);
            }
        }

        // go through all the unfocused faders
        foreach (KeyValuePair<GameObject, GameObject> unfocusedKeyValuePair in unzoomableFaders)
        {
            GameObject childObject = unfocusedKeyValuePair.Key;
            Material[] childMaterials = ExtractMaterialsFromFader(childObject);
            // if there is no target && the xray is not on && we're at the zoom-in point
            if (target == null && !xrayState && zoomTargetPct < zoomFadeMargin)
            {
                FadeMaterial(childMaterials, opaqueValue);
            }
            else // there is a target, fade (if necessary) the unfocused renderer
            {
                FadeMaterial(childMaterials, unfocusedValue);
            }
        }

    }

    void FadeMaterial(Material[] materials, float faderValue)
    {
        foreach (Material material in materials)
        {
            Color color = material.color;
            color.a = faderValue;
            material.color = color;
        }
    }

    #endregion


    #region Perspective

    void ResetPerspective()
    {
        // make sure there aren't any previously running instances
        CancelResetPerspective();
        // rotate to original rotation
        //RotateToward(startingRotation);
        // rotate to original position
        ZoomToward(zoomStartingPct);
    }


    void CancelResetPerspective()
    {
        StopCoroutine("RotateToward");
        StopCoroutine("ZoomToward");
    }

    #endregion


    #region Snap

    void StartSceneChangeForward()
    {
        if (ZoomInStarted != null)
        {
            if (target == null)
            {
                Debug.LogError("Starting snap into null");
            }
            else
            {
                ZoomInStarted(target.name);
            }
        }
    }


    void StartSceneChangeBackward()
    {
        if (ZoomOutStarted != null)
        {
            ZoomOutStarted(null);
        }
    }


    void FinishSceneChangeForward()
    {
        if (ZoomInFinished != null)
        {
            if (target == null)
            {
                Debug.LogError("Finishing snap into null");
            }
            else
            {
                ZoomInFinished(target.name);
            }
        }
    }


    void FinishSceneChangeBackward()
    {
        if (ZoomOutFinished != null)
        {
            ZoomOutFinished(null);
        }
    }

    #endregion


    #region Target

    void TargetInit()
    {
        // memorize GameObjects (for targetting)
        MemorizeTargets();

        // if there is no assigned target and only one possible target, force this to the target
        if (target == null && zoomableTargets.Count == 1)
        {
            target = zoomableTargets[0];
        }
    }


    void ChangeTarget(GameObject newTarget)
    {
        // if we're snapping, it's the end of the scene. Stop all incoming processes
        if (zoomIsSnapping) return;

        // are we retargeting on an object
        if (newTarget != null)
        {
            // change target to new object
            ChangeTargetToNewObject(newTarget);
        }
        else // retargeting on null
        {
            // change target to null
            ChangeTargetToNull();
        }

    }


    void ChangeTargetToNewObject(GameObject newTarget)
    {
        // if the new target is not actually targetable
        if (!zoomableTargets.Contains(newTarget)) return;

        //// if we are already targeting this object
        //if (target == newTarget)
        //{
        //    // push in on this object
        //    ZoomIn();
        //}

        // remember target
        target = newTarget;

        // set cinemachine to target this object as well
        SetFreeLookTarget(target.transform);

    }


    /// <summary>
    /// This is when we click in the void
    /// </summary>
    void ChangeTargetToNull()
    {
        // go back to original position
        ResetPerspective();

        // if we were previously targeted on an object and there is more than one target
        if (zoomableTargets.Count > 1)
        {
            // stop this selection
            target = null;
        }

        // set cinemachine to null target
        SetFreeLookTarget(unselectedTarget.transform);

        return;
    }


    /// <summary>
    /// Set the Free-Look virtual camera to a new target.
    /// Also, animate the transition with dampening.
    /// </summary>
    /// <param name="targetTransform">The new target's transform.</param>

    void SetFreeLookTarget(Transform targetTransform)
    {
        freeLookCamera.m_Follow = targetTransform;
        freeLookCamera.m_LookAt = targetTransform;
        // turn off previous dampening adjustment routine if it exists
        if (freeLookDampeningRoutine != null) StopCoroutine(freeLookDampeningRoutine);
        // turn on dampening for a bit
        freeLookDampeningRoutine = StartCoroutine(SetFreeLookDampening());
    }


    /// <summary>
    /// Turns the off free look damping.
    /// </summary>

    void AbortFreeLookDamping()
    {
        // turn off previous dampening adjustment routine if it exists
        if (freeLookDampeningRoutine != null) StopCoroutine(freeLookDampeningRoutine);
        // turn off damping
        SetSetFreeLookDampingComponents(0.0f);
        // turn off flag
        freeLookIsDamping = false;
    }


    /// <summary>
    /// Temporarily turns on dampening on the Free-Look camera
    /// </summary>
    /// <returns>The free look dampening.</returns>

    IEnumerator SetFreeLookDampening()
    {
        // turn on flag
        freeLookIsDamping = true;
        // set this damping value
        SetSetFreeLookDampingComponents(2.5f);
        // wait for the next frame
        yield return new WaitForSeconds(2.0f);
        // turn off damping
        SetSetFreeLookDampingComponents(0.0f);
        // all done, turn off flag
        freeLookIsDamping = false;
    }


    void SetSetFreeLookDampingComponents(float value)
    {
        // go through each rig
        for (int i = 0; i < 3; i++)
        {
            // get this rig
            CinemachineVirtualCamera rig = freeLookCamera.GetRig(i);
            // get the component of the rig that deals with tracking damping
            CinemachineOrbitalTransposer orbitalTransposer = rig.GetCinemachineComponent<CinemachineOrbitalTransposer>();
            // set the dampening
            orbitalTransposer.m_XDamping = value;
            orbitalTransposer.m_YDamping = value;
            orbitalTransposer.m_ZDamping = value;
            // get the component of the rig that deals with targetting damping
            CinemachineComposer composer = rig.GetCinemachineComponent<CinemachineComposer>();
            // set the damping
            composer.m_HorizontalDamping = value;
            composer.m_VerticalDamping = value;
        }
    }


    void SelectPreviousTarget(bool useless = true)
    {
        // if we're snapping, it's the end of the scene. Stop all incoming processes
        if (zoomIsSnapping) return;

        // start with an index of none (null)
        int currentIndex = -1;

        // if there is a current target
        if (target != null)
        {
            // remember current index
            currentIndex = zoomableTargets.IndexOf(target);
        }

        if (zoomableTargets.Count == 1)
        {
            currentIndex = 0;
        }
        // wrap around
        else if (currentIndex < 0)
        {
            currentIndex = zoomableTargets.Count - 1;
        }
        else  // otherwise
        {
            // just decrement
            currentIndex -= 1;
        }

        // apply changes
        SelectTarget(currentIndex);
    }


    void SelectNextTarget(bool useless = true)
    {
        // if we're snapping, it's the end of the scene. Stop all incoming processes
        if (zoomIsSnapping) return;

        // start with an index of none (null)
        int currentIndex = -1;
        // if there is a current target
        if (target != null)
        {
            // remember current index
            currentIndex = zoomableTargets.IndexOf(target);
        }
        // wrap around
        if (zoomableTargets.Count == 1)
        {
            currentIndex = 0;
        }
        else if (currentIndex >= zoomableTargets.Count - 1)
        {
            // set to null
            currentIndex = -1;
        }
        else // otherwise
        {
            // incremement
            currentIndex += 1;
        }

        // apply changes
        SelectTarget(currentIndex);
    }


    void SelectTarget(int targetIndex)
    {
        // if we're null
        if (targetIndex == -1)
        {
            target = null;
            // reset perspective to original position
            ResetPerspective();
            // set cinemachine to null target
            SetFreeLookTarget(unselectedTarget.transform);
        }
        else
        {
            // set the target using the index
            target = zoomableTargets[targetIndex];
            // set cinemachine to null target
            SetFreeLookTarget(target.transform);
        }

        // we've changed target state, so show currently available targetting options
        TurnOnXray();
    }

    #endregion


    #region Xray

    void TouchChanged(int touchCount, int previousTouchCount, int touchId)
    {
        // if we're touching down from touches off
        if (previousTouchCount == 0 && touchCount == 1 && !xrayState)
        {
            TurnOnXray();
        }
        // if we're releasing all touches 
        else if (touchCount > 1 && xrayState)
        {
            TurnOffXray();
        }
        // if xray was on and we're not longer touching
        else if (xrayState && touchCount == 0)
        {
            TurnOffXray();
        }

    }


    void TouchDragged(int touchCount)
    {
        if (xrayState)
        {
            TurnOffXray();
        }
    }


    /// <summary>
    /// Turns the X-Ray focus on, making clickable/zoomable objects visible
    /// </summary>

    void TurnOnXray()
    {
        xrayState = true;
        StartXray();
    }

    /// <summary>
    /// Turns the X-Ray focus off, making clickable/zoomable objects invisible
    /// </summary>

    void TurnOffXray()
    {
        xrayState = false;
        StartXray();
    }

    /// <summary>
    /// Turns the X-Ray focus on/off, making clickable/zoomable objects visible/invisible
    /// </summary>

    void ToggleXray()
    {
        xrayState = !xrayState;
        StartXray();
    }

    /// <summary>
    /// Starts the Xray routine
    /// </summary>

    void StartXray()
    {
        // turn off any other instances of this routine
        StopCoroutine("XrayRoutine");
        // start the routine to change xray
        StartCoroutine("XrayRoutine");
    }

    void ResetXrayCountdown()
    {
        //xrayCountdown = xrayDuration;
    }

    /// <summary>
    /// Animation co-routine to change opacity to new value
    /// </summary>

    IEnumerator XrayRoutine()
    {
        bool needToChangeOpacity = true;

        // if we're turning on xray
        if (xrayState)
        {
            // wait a little
            yield return new WaitForSeconds(0.1f);
        }

        while (needToChangeOpacity)
        {
            // calculate the xray values
            needToChangeOpacity = XrayChangeOpacity();
            // wait a cycle
            yield return new WaitForFixedUpdate();
        }

        //// so, did we just turn the xray on?
        //if (xrayState)
        //{
        //    // reset the xray timer to zero
        //    ResetXrayCountdown();

        //    // wait for xray countdown to finish
        //    while (xrayCountdown > 0.0f)
        //    {
        //        // countdown the timer
        //        xrayCountdown -= Time.deltaTime;
        //        // wait for frame
        //        yield return new WaitForEndOfFrame();
        //    }

        //    // now turn off
        //    xrayState = false;

        //    // reset requirement to change opacity
        //    needToChangeOpacity = true;

        //    while (needToChangeOpacity)
        //    {
        //        // calculate the xray values
        //        needToChangeOpacity = XrayChangeOpacity();
        //        // wait a cycle
        //        yield return new WaitForFixedUpdate();
        //    }
        //}

    }

    bool XrayChangeOpacity()
    {
        // if we're on, increase value
        if (xrayState == false) xrayOpacityValue += xraySpeed;
        else xrayOpacityValue -= xraySpeed; // or decrease

        // clamp values
        xrayOpacityValue = Mathf.Clamp(xrayOpacityValue, 0.1f, 1.0f);

        //Debug.Log(xrayOpacityValue);

        // apply these changes
        ApplyRendererOpacities();

        // change the background
        float backgroundLerpValue = 1.0f - xrayOpacityValue;
        SetBackgroundXrayColor(backgroundLerpValue);

        // calculate if we've reached the opacity goal
        if (xrayState == false && Mathf.Approximately(xrayOpacityValue, 1.0f))
        {   // ok, we're done
            return false;
        }
        else if (xrayState == true && Mathf.Approximately(xrayOpacityValue, 0.1f))
        {   // ok, we're done
            return false;
        }

        return true;
    }

    #endregion


    #region Tools

    float ConvertPctToZoom(float pctValue)
    {
        float zoomValue = pctValue;
        // scale up to zoom range
        zoomValue *= (zoomMaximum - zoomMinimum);
        // add bottom offset
        zoomValue += zoomMinimum;
        // return result
        return zoomValue;
    }


    float ConvertZoomToPct(float zoomValue)
    {
        return ((zoomValue - zoomMinimum) / (zoomMaximum - zoomMinimum));
    }

    #endregion


    #region Listeners

    /// <summary>
    /// Whenever this Object/script is enabled
    /// </summary>

    void OnEnable()
    {
        // start listening for various types of interaction

        // Movement
        HyperzoomInteraction.DidRotate += Rotate;
        HyperzoomInteraction.DidZoom += Zoom;
        HyperzoomInteraction.DidZoomIn += ZoomIn;
        HyperzoomInteraction.DidZoomOut += ZoomOut;
        HyperzoomInteraction.DidFinishZoom += ZoomCleanup;
        // Change of target
        HyperzoomInteraction.DidChangeTarget += ChangeTarget;
        HyperzoomInteraction.DidSelectNextTarget += SelectNextTarget;
        HyperzoomInteraction.DidSelectPreviousTarget += SelectPreviousTarget;
        // x-ray functions
        HyperzoomPointer.TouchChanged += TouchChanged;
        HyperzoomPointer.TouchDragged += TouchDragged;
    }


    /// <summary>
    /// Whenever this Object/script is disabled
    /// </summary>

    void OnDisable()
    {
        // stop listening for various types of interaction

        // Movement
        HyperzoomInteraction.DidRotate -= Rotate;
        HyperzoomInteraction.DidZoom -= Zoom;
        HyperzoomInteraction.DidZoomIn -= ZoomIn;
        HyperzoomInteraction.DidZoomOut -= ZoomOut;
        HyperzoomInteraction.DidFinishZoom -= ZoomCleanup;
        // Change of target
        HyperzoomInteraction.DidChangeTarget -= ChangeTarget;
        HyperzoomInteraction.DidSelectNextTarget -= SelectNextTarget;
        HyperzoomInteraction.DidSelectPreviousTarget -= SelectPreviousTarget;
        // x-ray functions
        HyperzoomPointer.TouchChanged -= TouchChanged;
        HyperzoomPointer.TouchDragged -= TouchDragged;
    }

    #endregion


}
