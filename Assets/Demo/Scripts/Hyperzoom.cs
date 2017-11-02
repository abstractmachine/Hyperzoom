using System.Collections;
using UnityEngine;
using Cinemachine;

public class Hyperzoom : MonoBehaviour
{
    #region Serialized Properties 

    [Header("References")]

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
    [Range(0.0f, 50.0f)]
    [SerializeField]
    private float zoomMinimum = 2.0f;

    /// <summary>
    /// The starting value for the virtual camera
    /// </summary>
    [Tooltip("Define the starting zoom value of the virtual camera")]
    [Range(0.0f, 50.0f)]
    [SerializeField]
    private float zoomStart = 10.0f;

    /// <summary>
    /// The zoomed-out value for the virtual camera
    /// </summary>
    [Tooltip("Define the zoom value of a zoomed-out camera")]
    [Range(0.0f, 50.0f)]
    [SerializeField]
    private float zoomMaximum = 20.0f;

    /// <summary>
    /// The speed multiplier for zooming the virtual camera
    /// </summary>
    [Tooltip("Define the speed multiplier for zooming in/out the virtual camera")]
    [SerializeField]
    private float zoomSpeed = 0.5f;

    #endregion


    #region Private Properties

    /// <summary>
    /// A coroutine that stops the previous rotation events if no new events
    /// come in after one frame. Used to control Cinemachine's Free-Look system.
    /// </summary>
    private Coroutine rotationStopRoutine = null;

    #endregion

    #region Zoom Properties

    private bool isOrthographic = false;

    //private float zoomStartingPct = 0.0f;
    private float zoomTarget = 0.5f;
    private float zoomFadeMargin = 0.25f;
    //private float zoomPointOfNoReturn = 0.5f;
    private bool zoomIsSnapping = false;
    //float zoomPointOfNoReturnLow = 0.0f;
    //float zoomPointOfNoReturnHigh = 1.0f;

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
        // Change of focus
        HyperzoomInteraction.DidChangeFocus += ChangeFocus;
        HyperzoomInteraction.DidSelectNextFocus += SelectNextFocus;
        HyperzoomInteraction.DidSelectPreviousFocus += SelectPreviousFocus;
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
        // Change of focus
        HyperzoomInteraction.DidChangeFocus -= ChangeFocus;
        HyperzoomInteraction.DidSelectNextFocus -= SelectNextFocus;
        HyperzoomInteraction.DidSelectPreviousFocus -= SelectPreviousFocus;
        // x-ray functions
        HyperzoomPointer.TouchChanged -= TouchChanged;
        HyperzoomPointer.TouchDragged -= TouchDragged;
    }

    #endregion


    #region Init

    void Start()
    {
        // base.Start();

        // FocusInit();

        // find out if we're orthographic
        CinemachineBrain brain = CinemachineCore.Instance.FindPotentialTargetBrain(freeLookCamera);
        isOrthographic = (brain != null) ? brain.OutputCamera.orthographic : false;

        // force the camera to the starting value
        if (isOrthographic) freeLookCamera.m_Lens.OrthographicSize = zoomStart;
        else freeLookCamera.m_Lens.FieldOfView = zoomStart;

        // remember this starting value
        zoomTarget = ConvertZoomToPct(zoomStart);

        // adjust speed if we're an orthographic camera
        if (isOrthographic) zoomSpeed *= 0.2f;

        // figure out the points of no-return
        //zoomPointOfNoReturnLow = zoomFadeMargin * zoomPointOfNoReturn;
        //zoomPointOfNoReturnHigh = 1.0f - (zoomFadeMargin * zoomPointOfNoReturn);

        // fade in scene
        //HideEverything();
        //StartCoroutine("FadeInEverything");

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
        Vector2 rotationDelta = new Vector2(delta.x, delta.y);
        rotationDelta.x *= (1000.0f / (float)Screen.width) * 0.0002f;
        rotationDelta.y *= (1000.0f / (float)Screen.height) * 0.0002f;

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

        // turn off any snapping to original perspective
        //CancelResetPerspective();

        // slow down delta
        delta = ZoomSlowDownDelta(delta);
        // first apply this value to our target
        zoomTarget += delta * zoomSpeed;
        // clamp the field of view to make sure it doesn't go beyond 0.0f and 1.0f
        zoomTarget = Mathf.Clamp(zoomTarget, 0.05f, 0.95f);

        // zoom to this new value
        ZoomToPct(zoomTarget);

        // reset the xray countdown (if it's waiting)
        //TurnOffXray();
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
        //ApplyRendererOpacities();
    }


    float ZoomSlowDownDelta(float delta)
    {
        // if we're at the lower level and delta is shrinking
        if (zoomTarget < zoomFadeMargin && delta < 0)
        {
            // normalize zoom value within the zoom margin
            float factor = ((zoomFadeMargin - zoomTarget) / zoomFadeMargin);
            // invert normalization
            factor = 1.0f - factor;
            // logarithmicize factor
            factor *= factor;
            // apply factor to delta
            return delta * factor;
        }

        if (zoomTarget > 1.0f - zoomFadeMargin && delta > 0)
        {
            // normalize zoom value within the zoom margin
            float factor = (1.0f - zoomTarget) / zoomFadeMargin;
            // logarithmicize factor
            factor *= factor;
            // apply factor to delta
            return delta * factor;
        }

        // return results
        return delta;
    }

    #endregion


    #region Handlers


    /// <summary>
    /// This handler is called whenever there is a specified "ZoomIn" request
    /// </summary>
    /// <param name="useless">Ingore this parameter.</param>

    void ZoomIn(bool useless = false)
    {
        //Debug.Log("ZoomIn");
    }


    /// <summary>
    /// This handler is called whenever there is a specified "ZoomOut" request
    /// </summary>
    /// <param name="useless">Ingore this parameter.</param>

    void ZoomOut(bool useless = false)
    {
        //Debug.Log("ZoomOut");
    }


    /// <summary>
    /// this method is called when we've finished or paused zooming
    /// </summary>
    /// <param name="useless">Ingore this parameter.</param>

    void ZoomCleanup(bool useless = false)
    {
        //Debug.Log("ZoomCleanup");
    }


    /// <summary>
    /// This handler is called whenever there is a request for a change of target
    /// </summary>
    /// <param name="newTarget">New target.</param>

    void ChangeFocus(GameObject newTarget)
    {
        //Debug.Log("ChangeFocus");
    }


    /// <summary>
    /// This handler is called whenever there is a request to move to the next focus target
    /// </summary>
    /// <param name="useless">Ingore this parameter.</param>

    void SelectNextFocus(bool useless = true)
    {
        //Debug.Log("SelectNextFocus");
    }


    /// <summary>
    /// This handler is called whenever there is a request to move to the previous focus target
    /// </summary>
    /// <param name="useless">Ingore this parameter.</param>

    void SelectPreviousFocus(bool useless = true)
    {
        //Debug.Log("SelectPreviousFocus");
    }

    #endregion


    #region X-Ray

    /// <summary>
    /// This handler is called whenever there has been a change of number of touches
    /// </summary>
    /// <param name="touchCount">The current number of active touches.</param>
    /// <param name="previousTouchCount">The Previous number of active touched.</param>
    /// <param name="touchId">The index ID of the finger that changed.</param>

    void TouchChanged(int touchCount, int previousTouchCount, int touchId)
    {
        //Debug.Log("TouchChanged");
    }


    /// <summary>
    /// This handler is called whenever there has finger mouvement
    /// </summary>
    /// <param name="touchCount">Touch count.</param>

    void TouchDragged(int touchCount)
    {
        //Debug.Log("TouchDragged"); 
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


}
