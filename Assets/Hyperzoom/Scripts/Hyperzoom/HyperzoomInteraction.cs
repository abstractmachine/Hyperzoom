using System;
using UnityEngine;

public class HyperzoomInteraction : MonoBehaviour
{

    #region Properties

    /// <summary>
    /// The Camera attached to the gameObject using the Target script
    /// </summary>
    //private Camera currentCamera = null;

    #endregion


    #region Subclassed Events

    /// <summary>
    /// Whenever we've clicked on a targetable GameObject, fire this event
    /// </summary>
    public static event Action<GameObject> DidChangeTarget;

    /// <summary>
    /// This relays events from Interaction subclasses
    /// Whenever we've clicked on a targetable GameObject, fire this event
    /// </summary>
    /// <param name="newTargetObject">New target object.</param>
    protected virtual void ChangedTarget(GameObject newTargetObject)
    {
        Action<GameObject> handler = DidChangeTarget;
        if (handler != null) handler(newTargetObject);
    }

    /// <summary>
    /// Whenever we click & drag, send out this event
    /// </summary>
    public static event Action<GameObject, Vector3> DidRotate;

    /// <summary>
    /// This relays events from Interaction subclasses
    /// Whenever we click & drag, send out this event
    /// </summary>
    /// <param name="targetedObject">The GameObject we rotated around.</param>
    /// <param name="delta">The values of the delta rotation.</param>
    protected virtual void Rotated(GameObject targetedObject, Vector3 delta)
    {
        Action<GameObject, Vector3> handler = DidRotate;
        if (handler != null) handler(targetedObject, delta);
    }

    /// <summary>
    /// Whenever we scroll || pinch, fire this event
    /// </summary>
    public static event Action<float> DidZoom;

    /// <summary>
    /// This relays events from Interaction subclasses
    /// Whenever we scroll || pinch, fire this event
    /// </summary>
    /// <param name="float">The value for the zoom delta.</param>
    protected virtual void Zoomed(float zoomDelta)
    {
        Action<float> handler = DidZoom;
        if (handler != null) handler(zoomDelta);
    }

    ///<summary>
    /// When we touch/click up — or scrolling time-outs —, fire this event
    ///</summary>
    ///<returns>The finish zoom.</returns>
    public static event Action<bool> DidFinishZoom;

    /// <summary>
    /// This relays events from Interaction subclasses
    /// When we touch/click up — or scrolling time-outs —, fire this event
    /// </summary>
    /// <param name="float">The value for the zoom delta.</param>
    protected virtual void FinishedZoom(bool didFinish = true)
    {
        Action<bool> handler = DidFinishZoom;
        if (handler != null) handler(didFinish);
    }

    /// <summary>
    /// A specified "ZoomIn" event, usually fired from a button press
    /// </summary>
    public static event Action<bool> DidZoomIn;

    /// <summary>
    /// This relays events from Interaction subclasses
    /// A specified "ZoomOut" event, usually fired from a button press
    /// </summary>
    /// <param name="didZoom">A useless flag.</param>
    protected virtual void ZoomedIn(bool didZoom = true)
    {
        Action<bool> handler = DidZoomIn;
        if (handler != null) handler(didZoom);
    }

    /// <summary>
    /// A specified "ZoomOut" event, usually fired from a button press
    /// </summary>
    public static event Action<bool> DidZoomOut;

    /// <summary>
    /// This relays events from Interaction subclasses
    /// A specified "ZoomOut" event, usually fired from a button press
    /// </summary>
    /// <param name="didZoom">A useless flag.</param>
    protected virtual void ZoomedOut(bool didZoom = true)
    {
        Action<bool> handler = DidZoomOut;
        if (handler != null) handler(didZoom);
    }

    /////////////////////////////////////////
    /// 
    /// <summary>
    /// buttons have fired SelectNextTarget event
    /// </summary>
    public static event Action<bool> DidSelectNextTarget;

    /// <summary>
    /// This relays events from Interaction subclasses
    /// A specified "DidSelectNextTarget" event, usually fired from a button press
    /// </summary>
    /// <param name="didSelect">A useless flag.</param>
    protected virtual void SelectedNextTarget(bool didSelect = true)
    {
        Action<bool> handler = DidSelectNextTarget;
        if (handler != null) handler(didSelect);
    }

    /// <summary>
    /// buttons have fired SelectPreviousTarget event
    /// </summary>
    public static event Action<bool> DidSelectPreviousTarget;

    /// <summary>
    /// This relays events from Interaction subclasses
    /// A specified "DidSelectPreviousTarget" event, usually fired from a button press
    /// </summary>
    /// <param name="didSelect">A useless flag.</param>
    protected virtual void SelectedPreviousTarget(bool didSelect =true)
    {
        Action<bool> handler = DidSelectPreviousTarget;
        if (handler != null) handler(didSelect);
    }

    #endregion


    #region Click

    /// <summary>
    /// if is triggered, this is not a target-able object
    /// </summary>

    public virtual void PointerClicked()
    {
        Debug.LogError("Unhandled PointerClicked() event");
    }

    #endregion

}