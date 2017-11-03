using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HyperzoomPointer : HyperzoomInteraction, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IScrollHandler
{
    #region Pointer class

    /// <summary>
    /// This Point class is used for tracking information on each pointer (mouse & touch)
    /// </summary>
    public class PointerData
    {
        public int id = -1;
        public Vector2 start = Vector2.zero;
        public Vector2 position = Vector2.zero;
        public Vector2 previous = Vector2.zero;
        public Vector2 delta = Vector2.zero;
        public float startTime = 0.0f;
    }

    #endregion


    #region Properties

    /// <summary>
    /// The x,y multiplier affects touch rotations. The z multiplier affects pinch zooms
    /// </summary>
    private Vector2 rotationMultiplier = Vector2.one;

    /// <summary>
    /// This multiplier to applied to mouse scroll-wheel zooms
    /// </summary>
    private float scrollWheelMultiplier = 0.5f;

    /// <summary>
    /// This multiplier to applied to multitouch pinched zooms
    /// </summary>
    private float pinchMultiplier = 0.015f;

    /// <summary>
    /// tracks whether we're dragging or not
    /// </summary>
    protected static bool didDrag = false;

    /// <summary>
    /// How many pixels do we have to drag to activate didDrag?
    /// </summary>
    protected static float dragActivationDistance = 40.0f;

    /// <summary>
    /// Tracks whether we're zooming or not
    /// </summary>
    protected static bool didZoom = false;

    /// <summary>
    /// Tracks whether the player held the click or not
    /// </summary>
    protected static bool didHold = false;

    /// <summary>
    /// How long it takes to declare a "hold"
    /// </summary>
    protected static float holdDelay = 0.6f;
    protected static float holdStart = 0.0f;

    /// <summary>
    /// Tracks whether we used multiple fingers
    /// </summary>
    protected static bool didMultitouch = false;

    /// <summary>
    /// The list of IDs for all the current fingers touching the screen
    /// </summary>
    protected static Dictionary<int, PointerData> pointerPositions = new Dictionary<int, PointerData>();

    /// <summary>
    /// how long we wait before considering that interaction has timed out.
    /// </summary>
    private float didInteractTimeoutDelay = 1.0f;

    #endregion


    #region Events

    public static event Action<int, int, int> TouchChanged;
    public static event Action<int> TouchDragged;

    #endregion


    #region Drag

    public void OnBeginDrag(PointerEventData eventData)
    {
        //didDrag = true;
    }


    /// <summary>
    /// Handle drag events on this object
    /// </summary>
    /// <param name="eventData">The details of the user's interaction</param>

    public void OnDrag(PointerEventData eventData)
    {
        // avoid treating micro-movements as drag events
        // if we aren't dragging yet, and there is a starting point recorded for this pointer
        if (!didDrag && pointerPositions.ContainsKey(eventData.pointerId))
        {
            // calculate distance from starting point
            Vector2 startDelta = eventData.position - pointerPositions[eventData.pointerId].start;
            // if the drag is large enough, or we're multitouch (pinching)
            if (startDelta.magnitude > dragActivationDistance || pointerPositions.Count > 1)
            {
                // activate this "yes we did drag" flag
                didDrag = true;
                // if there  are any listeners
                if (TouchDragged != null)
                {
                    // tell the listeners how many fingers there are
                    TouchDragged(pointerPositions.Count);
                }
            }
        }

        // if there are any listeners wanting to know if we've just dragged on this object
        if (didDrag)
        {
            // remember the position of this pointer
            // make sure this pointer ID doesn't already
            if (pointerPositions.ContainsKey(eventData.pointerId))
            {
                // remember previous position
                pointerPositions[eventData.pointerId].previous = pointerPositions[eventData.pointerId].position;
                // calculate delta using previous position
                pointerPositions[eventData.pointerId].delta = eventData.position - pointerPositions[eventData.pointerId].previous;
                // update new position
                pointerPositions[eventData.pointerId].position = eventData.position;
            }

            // if there are multitouches and this is #3, or there's only one touch
            if (pointerPositions.Count < 2 /*|| eventData.pointerId == 0*/)
            //if (pointerPositions.Count < 2)
            {
                // calculate final value using pointer multiplier
                Vector2 rotationDelta = eventData.delta;
                rotationDelta.x *= rotationMultiplier.x;
                rotationDelta.y *= rotationMultiplier.y;
                // invert direction
                rotationDelta.x *= -1.0f;
                rotationDelta.y *= -1.0f;
                // rotate around focus pointusing delta data
                Rotated(this.gameObject, rotationDelta);
                // flag interaction
                DidInteract();
            }

            // if we're multitouch
            if (pointerPositions.Count == 2)
            {
                // calculate the delta changes to the pinch
                CalculatePinch();
                // flag interaction
                DidInteract();
            }

        } // if (DidDrag != null
    }
    // OnDrag()


    public void OnEndDrag(PointerEventData eventData)
    {
        //        // turn off the drag flag
        //        didDrag = false;
    }

    #endregion


    #region Scroll

    public void OnScroll(PointerEventData eventData)
    {
        // extract delta from scroll
        Vector2 delta = eventData.scrollDelta;
        // extract the position
        Vector2 position = eventData.position;
        // re-calculate scroll speed
        delta.y *= scrollWheelMultiplier;

        // if scrolling with the mouse
        if (!Mathf.Approximately(Mathf.Abs(delta.y), 0.0f))
        {
            // apply zoom action
            Zoomed(-delta.y);
            // flag interaction
            DidInteract();

            // set the didZoom flag
            didZoom = true;
        }
        // if (!Mathf
    }

    #endregion


    #region Multitouch

    void CalculatePinch()
    {
        // Find the magnitude of the vector (the distance) between the touches in each frame
        float previousTouchDeltaMagnitude = (pointerPositions[0].previous - pointerPositions[1].previous).magnitude;
        float currentTouchDeltaMagnitude = (pointerPositions[0].position - pointerPositions[1].position).magnitude;

        // Find the difference in the distances between each frame.
        float zoomDelta = previousTouchDeltaMagnitude - currentTouchDeltaMagnitude;

        // apply speed dampener
        zoomDelta *= pinchMultiplier;

        // pinch speed is different depending on direction
        if (zoomDelta > 0) zoomDelta *= 0.75f;

        // send out pinch event
        //DidPinch(delta);
        Zoomed(zoomDelta);
        // set the didZoom flag
        didZoom = true;
        // flag interaction
        DidInteract();
    }

    #endregion


    #region PointerChange

    public void OnPointerDown(PointerEventData eventData)
    {
        // get the previous amount of pointers
        int previousCount = pointerPositions.Count;

        // if we need to reset flags
        if (previousCount == 0)
        {
            ResetFlags();
        }

        // create a new Pointer object
        // make sure this pointer ID doesn't already
        if (!pointerPositions.ContainsKey(eventData.pointerId))
        {
            PointerData pointerData = new PointerData();
            pointerData.id = eventData.pointerId;
            pointerData.start = eventData.pressPosition;
            pointerData.previous = eventData.position;
            pointerData.position = eventData.position;
            pointerData.startTime = Time.time;
            // remember this position
            pointerPositions.Add(eventData.pointerId, pointerData);
        }

        // if we used multiple fingers
        if (pointerPositions.Count > 1)
        {
            // remember that we're multitouching
            didMultitouch = true;
        }

        // if there are any listeners
        if (TouchChanged != null)
        {
            // send out how many fingers/mouse are touching the screen
            TouchChanged(pointerPositions.Count, previousCount, eventData.pointerId);
        }
    }
    // OnPointerDown

    public void OnPointerUp(PointerEventData eventData)
    {

        // get the previous amount of pointers
        int previousCount = pointerPositions.Count;

        // check to see if this key exists in the dictionary
        if (pointerPositions.ContainsKey(eventData.pointerId))
        {
            // get the start time of this click
            float timeLength = Time.time - pointerPositions[eventData.pointerId].startTime;
            // determine if this click was long enough to be considered a hold
            if (timeLength >= holdDelay)
            {
                // activate hold flag
                didHold = true;
                holdStart = Time.time;
            }

            // remove it
            pointerPositions.Remove(eventData.pointerId);
        }

        // if we neither zoomed nor dragged nor used any multitouch and now we're all released
        if (!didDrag && !didZoom && !didMultitouch && pointerPositions.Count == 0)
        {
            // activate the selection click
            PointerClicked();
        }

        // if we were zooming and now there are not longer any fingers
        if (didZoom && pointerPositions.Count == 0)
        {
            // send out a pinch done event
            FinishedZoom(true);
        }

        // if we need to reset flags (and since we've already used them in PointerClicked)
        if (pointerPositions.Count == 0)
        {
            ResetFlags();
        }

        // if there are any listeners
        if (TouchChanged != null)
        {
            // send out how many fingers/mouse are touching the screen, and which finger changed
            TouchChanged(pointerPositions.Count, previousCount, eventData.pointerId);
        }
    }
    // OnPointerDown


    void ResetFlags()
    {
        didZoom = false;
        didDrag = false;
        didHold = false;
        didMultitouch = false;
    }


    #endregion


    #region Timeout

    void DidInteract()
    {
        // if there was a previous Timeout routine waiting to run
        StopCoroutine("DidInteractTimeoutRoutine");
        // start the routine to count down
        StartCoroutine("DidInteractTimeoutRoutine");
    }


    IEnumerator DidInteractTimeoutRoutine()
    {
        // wait for however long we need to wait
        yield return new WaitForSeconds(didInteractTimeoutDelay);
        // send out a zoom done event
        FinishedZoom(true);
        // all done
        yield return null;
    }

    #endregion

}
